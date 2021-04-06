using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ViveSR.anipal.Eye;
using Tobii.XR;


//P.S. Os nomes de variaveis e metodos podem estar confusos> estao misturados termos em pt-br e eng.
public class StoreViewpoints : MonoBehaviour
{
    
    public bool EmExecucao = true;
    public bool showHeatmap = false;
    public bool updateEmTempoReal;
    public bool somenteRastro;
    public int tamanhoDoRastro=10;
    public float tempoParaUpdate;
    public float escalaRelativaDosPontos = 1;
    //Prefabs, podem ser privados, encontrando-os em Start()
    public GameObject efeitoHeatmap, efeitoHeatmapLaranja, efeitoHeatmapVermelho; 
    public GameObject colisor;
    //Salvar o Heatmap
    public bool salvarHeatmap = false;
    //Para visualizacao de gravacoes anteriores
    public bool ModoPlayer;
    public string CaminhoDoArquivoDeReplay;


    //Controle de parametros
    private bool EmExecucao_background = true;
    private bool ModoPlayer_anterior = false;
    private GameObject objetoPai;
    private bool _ultimoShowHeatmap, _ultimoSalvarHeatmap;
    public Color[] corNova = new Color[3];//Verificar como o parametro aparece para o usuário
    public float opacidade;


    //Camera principal ou HMD
    private bool HMD = false;
    private Camera cam;

    [SerializeField]
    private RaycastHit pontoAtual, pontoAnterior;
    

    //Variaveis para a personalizacao dos pontos (escala, cor, opacidade) 
    private GameObject[] e_H_novo = new GameObject[3]; //efeito_Heatmap_novo
    private float escalaRelativaDosPontos_anterior = 1;
    private Vector3[] escalaOriginal = new Vector3[3];
    private Renderer[] _renderer = new Renderer[3];
    private Color[] corOriginal = new Color[3];
    private Color[] corAnterior = new Color[3];
    private float opacidadeOriginal, opacidadeAnterior;



    //Armazenando ObjetoParent, listas de pontos e seus objetos e uma fila para o rastro.
    protected List<Vector3> pontos = new List<Vector3>();
    protected List<GameObject> objetosHeatmap = new List<GameObject>();
    //protected List<GameObject> colliders = new List<GameObject>(); //Para encontrar objetos que foram destruidos por pouca utilidade
    protected Queue<GameObject> rastro = new Queue<GameObject>();


    //Usando o BinaryManager para guardar os dados
    private ItemEntry user;
    private List<InfoperSecond> infoHeatmap = new List<InfoperSecond>();
    protected List<float> tempos = new List<float>();//Para sincronizar tempo do player com o arquivo de replay
    //Auxiliares para replay
    protected int indexPontos=0 , indexTempos=0;



    private void Start()
    {
        objetoPai = gameObject; //Associa o objeto que contem o script
           //TO DO 
        //Criando objetos de heatmap modificaveis no contexto (pode entrara em desuso, caso não se modifique muito e armazenando os valores originais como na escala)
        //e_H_novo = GameObject.Find("efeitoHeatmap");
        //GameObject.Instantiate(e_H_novo,new Vector3(1,1,1), Quaternion.identity);Debug.Log("INSTANCIADO EM 1,1,1");
        e_H_novo[0] = GameObject.Instantiate(efeitoHeatmap        ,Vector3.zero, Quaternion.identity, objetoPai.transform) as GameObject; 
        e_H_novo[1] = GameObject.Instantiate(efeitoHeatmapLaranja ,Vector3.zero, Quaternion.identity, objetoPai.transform) as GameObject; 
        e_H_novo[2] = GameObject.Instantiate(efeitoHeatmapVermelho,Vector3.zero, Quaternion.identity, objetoPai.transform) as GameObject; 
        // GameObject.Instantiate(new GameObject("e_H_novo1"),Vector3.zero, Quaternion.identity, objetoPai.transform);
        // GameObject.Instantiate(new GameObject("e_H_novo2"),Vector3.zero, Quaternion.identity, objetoPai.transform);
        // e_H_novo[0] = new GameObject("e_H_novo", efeitoHeatmap.GetComponents(Collider)) ;
        // GameObject.Instantiate(e_H_novo[0],Vector3.zero, Quaternion.identity, objetoPai.transform);
        // e_H_novo[0] = GameObject.Find("e_H_novo0");
        //e_H_novo[0] = efeitoHeatmap.gameObject; //.gameObject parece ser diferente
        //e_H_novo[1] = efeitoHeatmapLaranja;
        //e_H_novo[2] = efeitoHeatmapVermelho;
        //UnityEditor.PrefabUtility.Get
       
        //Escala (testar for loop)
        escalaOriginal[0] = efeitoHeatmap.transform.localScale;
        escalaOriginal[1] = efeitoHeatmapLaranja.transform.localScale;
        escalaOriginal[2] = efeitoHeatmapVermelho.transform.localScale;

        //Cor e opacidade do Heatmap (tres tipos)
        for(int n=0; n<3; n++)
        {
            _renderer[n] = e_H_novo[n].GetComponent<Renderer>(); //Só pega o verde por enquanto //
            corOriginal[n] = _renderer[n].sharedMaterial.color;
            corAnterior[n] = corOriginal[n];
        }
        //Descobrir como saber se houve entradas
        if(true){corNova=corOriginal;}//Parametros de cor não especificados, assumimos o original
        
        opacidadeOriginal = opacidadeAnterior = corOriginal[0].a;


        //Checando se o HMD esta funcionando corretamente e se pode ser usado
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                 SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
        {
            HMD = false;
        }
        else HMD = true;

    }


    // Update é chamado a cada frame, mas se respeita o tempo para execucao
    void Update()
    {
        //Inicia em segundo plano a rotina de captura de pontos
        if (EmExecucao && EmExecucao_background)
        {
            StartCoroutine(storePoints());
        }
        //Para modo replay em tempo real
        else if (ModoPlayer!=ModoPlayer_anterior && ModoPlayer==true)
        {
            EmExecucao = EmExecucao_background = false;
            Debug.Log("Modo Player ativo - Novas entradas de Heatmap não são aceitas,"
                                        +" construido a partir do arquivo fornecido");
            
            //Constroi listas de pontos e tempos
            playerHeatmapArquivo();

            //Para modo de replay -> Lê o arquivo e faz aquisicao dos dados salvos
            if(updateEmTempoReal) //Em tempo real
            {
                if (indexPontos < pontos.Count)//Verifica se a contagem nao chegou ao fim
                {
                     //Só imprime quando não é em tempo real
                      StartCoroutine(replayTempoReal());
                }else StopCoroutine(replayTempoReal());    
            }
            else{   //Constroi inteiro do arquivo
                    imprimirHeatmap();
                }
            
        }
        ModoPlayer_anterior = ModoPlayer; //Garantindo a detecção de mudanças, que ativa o replay quando ModoPlayer = true
            


        
        //Ativar/desativar a visualizacao do heatmap
        if (_ultimoShowHeatmap!=showHeatmap)
        {
            _ultimoShowHeatmap=showHeatmap;
            //Caso desejado, chama o metodo de impressao do heatmap
            if (showHeatmap) { imprimirHeatmap(); }
            else { apagarHeatmap(); }
        }
        
        //Metodo para salvar o arquivo imediatamente
        if (_ultimoSalvarHeatmap!=salvarHeatmap)
        {
            _ultimoSalvarHeatmap=salvarHeatmap;
            gravarDadosCompletos(); 
            Debug.Log("Os pontos armazenados foram salvos"); 
        }

        //Se houver mudança de escala dos pontos de heatmap: //Cor tambem pode ser implementada assim
        if (escalaRelativaDosPontos_anterior != escalaRelativaDosPontos) 
        {
            escalaRelativaDosPontos_anterior = escalaRelativaDosPontos;
            mudarEscala(escalaRelativaDosPontos);
        }

        //Se houver mudança de cor ou opacidade
        if (opacidade!=opacidadeAnterior || !corNova.Equals(corAnterior))
        {
            opacidadeAnterior = opacidade;
            corAnterior = corNova;
            mudarCorEOpacidade();
        }
    
    }

    


    //Salva os dados com o BinaryManager ao sair da aplicacao / Retornar valores originais dos prefabs
    void OnApplicationQuit()
    {
        if (!ModoPlayer) { //Somente no modo normal, o modo replay nao deve armazenar novamente os dados
            BinaryManagerSave();
            Debug.Log("Aplicacao parada e dados salvos");
        }

        //Retornando as escalas corretas dos prefabs
        mudarEscala(1);
    }



    //Aquisição dos pontos de visao e armazenamento desses - com eventual impressao individual
    public IEnumerator storePoints()
    {
        EmExecucao_background = false;//paralisa o update e continua o processo ao longo dos frames
        //Debug.Log("Entrou em execucao");
            //TO DO: dwell-time
        //Espera um periodo predeterminado para executar novamente a captura
        yield return new WaitForSecondsRealtime(tempoParaUpdate);
        //Debug.Log("deve ter esperado o tempo até update");

        //Escolha da camera
        if (!HMD) //Caso nao esteja funcionando o headset
        {
            cam = Camera.main;//Camera prinicipal da cena
            //Captura do ponto de visao projetado no mundo
            pontoAtual = capturaPonto(cam);
        }
        else //para o HMD
        {
            pontoAtual = capturaPontoHMD();
        }

        if (pontoAnterior.point == pontoAtual.point) { }//RaycastHit.ReferenceEquals(pontoAnterior,pontoAtual)) { }
        else//se existir ponto ele sera armazenado
        {
            //Debug.Log("Ponto atual: " + pontoAtual.point);
            //Debug.Log("Ponto anterior: " + pontoAnterior.point);
            
            //Armazena pontos em formato Vector3 para construir o heatmap
            pontos.Add(pontoAtual.point);
            pontoAnterior = pontoAtual;

            //Rastro e atualizacao em tempo real
            if (somenteRastro)
            {
                if (!(tamanhoDoRastro > rastro.Count))//Se não houver mais espaço na fila
                {
                    //Apagar o ponto que foi dequeued
                    GameObject.Destroy(rastro.Dequeue());
                    //Coloca o novo ponto com enqueue
                    rastro.Enqueue(imprimirPontoSimples(pontoAtual.point));
                }
                else //Aumenta a fila com o novo ponto
                {
                    rastro.Enqueue(imprimirPontoSimples(pontoAtual.point));
                }
            }
            else if(updateEmTempoReal)//Para impressao em tempo real ativa
            {
                imprimirPontoIndividualmente(pontoAtual.point);
            }
         


            //Salvar://Lista de InfoperSecond para o ItemEntry do BinaryManager //Atualmente com as infos de EyeT zeradas
            infoHeatmap.Add(new InfoperSecond(Time.time, pontoAtual.point,
                                        Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero));
        }


        EmExecucao_background = true;//Volta a ativacao pela rotina de update
    }


    private RaycastHit capturaPonto(Camera cam)
    {
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));//Deprecated?
        RaycastHit pontoNoMundo;
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))//Distância arbitraria demais?
        {
            return pontoNoMundo;
        }
        else return pontoAnterior;
    }

    private RaycastHit capturaPontoHMD()
    {
        var gazeRay = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay;
        if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.IsValid)//Verfica a validade dos dados. Redundante com o raycast?
        {

            Ray raio = new Ray(TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.Origin,
                                    TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.Direction);
            RaycastHit pontoNoMundo;
            if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))
            {
                return pontoNoMundo;
            }
            else return pontoAnterior;
        }
        else return pontoAnterior;
    }

    private void imprimirHeatmap()//Apaga o que esta impresso e imprime todos os pontos armazenados 
    {
        //Limpando os objetos ja impressos
        apagarHeatmap();

        //Inicialmente so' imprime os pontos armazenados, sem considerar proximidade e temperatura
        foreach (Vector3 ponto in pontos) {
            imprimirPontoIndividualmente(ponto);
        }
       
    }

    protected void imprimirPontoIndividualmente (Vector3 ponto)
    {
        if (Vector3.Equals(ponto, Vector3.zero)) { return; }//Nao deve imprimir o ponto nulo

        //Partindo do ponto e de um raio de anaise escolhido busca-se quantas colisoes ocorrem para determinar a temperatura do heatmap
        float raio = (float)(0.5*tempoParaUpdate); //Usa o tempo de update para adequar a proximidade relativa para a temperatura
        int layer = LayerMask.GetMask("Heatmap").GetHashCode();
        Collider[] hitColliders = new Collider[12];
        int colisoes = Physics.OverlapSphereNonAlloc(ponto, raio, hitColliders, layer);//Só detecta objetos na camada "Heatmap"
        //Debug.Log("Numero de colisoes detectadas proximo ao ponto: " + colisoes + "\n");
        
        //Apagar pontos desnecessarios
        if (colisoes > 6) { apagarPontosFrios(hitColliders); }

        //Escolher qual o efeito de heatmap para o numero de colisões encontradas no ponto
        GameObject efeitoHeatmapEscolhido;
        switch (colisoes)
        {
            case int n when (n >= 5 && n < 12):
                efeitoHeatmapEscolhido = e_H_novo[1];
                break;
            case int n when (n >= 12):
                efeitoHeatmapEscolhido = e_H_novo[2];
                break;
            default:
                //efeitoHeatmapEscolhido = efeitoHeatmap;
                efeitoHeatmapEscolhido = e_H_novo[0];
                break;
        }
       
        //Lista de objetos e criacao 
        objetosHeatmap.Add(Instantiate(efeitoHeatmapEscolhido, ponto, Quaternion.identity, objetoPai.transform));
    
    }

    //Apaga pontos desnecessarios, mas mantem um collider em seus lugares para calcular proximas temperaturas
    private void apagarPontosFrios(Collider[] hitColliders)
    {
        int quantidade = hitColliders.Length;
        for (int n=0; n<quantidade; n++)
        {
            GameObject objeto = hitColliders[0].gameObject;
            //Cria uma colisao no lugar do ponto
            objetosHeatmap.Add(Instantiate(colisor, objeto.transform.position, Quaternion.identity, objetoPai.transform));
            GameObject.Destroy(objeto);

        }
        return;
    }



    //Alternativamente, podemos somente imprimir um ponto padrao para todos sem logica extra (usado para retornar o objeto criado):
    private GameObject imprimirPontoSimples(Vector3 ponto) {
        //Não toma nota do ponto nulo, esse caso pode ser implementado pelo metodo que chamar este
        //Instancia 
        GameObject pontoCriado = Instantiate(efeitoHeatmap, ponto, Quaternion.identity, objetoPai.transform);
        objetosHeatmap.Add(pontoCriado);
        return pontoCriado;
    }



    private void apagarHeatmap()
    {
        foreach (GameObject objeto in objetosHeatmap) {
            GameObject.Destroy(objeto);
        }
        //Para reinstanciar os objetos para replay em tempo real:
        indexPontos = indexTempos = 0;

    }



    private void BinaryManagerSave()
    {
        //Inicialmente o nome esta como "nome" e os dados de EyeT estao zerados para testes (o construtor utilizado para isso é personalizado)
        user = new ItemEntry("nome", infoHeatmap);

        string endereco = getEndereco();

        BinaryManager.SaveInfo(user, endereco);
    }



    //Depreciado no momento, alternativamente seria para armazenar somente a lista de pontos do heatmap
    private void gravarDadosCompletos()
    {
        //Salvar todos os pontos do heatmap. CRIA UM NOVO ARQUIVO ARMAZENANDO TODOS OS PONTOS ATE O MOMENTO (incluindo pontos q estao em arquivos anteriores)
        BinaryManagerSave();
    }


    private string getEndereco()
    {
        string pasta, endereco;

        //Busca ou cria a pasta "ET Gravacoes"
        pasta = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), "\\ET Gravacoes\\");
        if (!System.IO.Directory.Exists(pasta)) { System.IO.Directory.CreateDirectory(pasta); }


        // Seu nome é "heatmap" + Data e horário do experimento
        endereco = pasta;
        endereco = string.Concat(@endereco, "heatmap");
        //endereco = string.Concat(@endereco, NavegarEntreCenasExperimento.getNome());
        endereco = string.Concat(@endereco, System.DateTime.Now.ToString("__dd_MM_yyyy__HH_mm_ss"));
        endereco = string.Concat(@endereco, ".txt");//ou .sav

        return endereco;
    }


    //Inicialmente só imprime todos os pontos, sem tomar nota do instante de tempo de cada, caso o updateEmTempoReal esteja desabilitado
    private void playerHeatmapArquivo()
    {
        infoHeatmap = BinaryManager.LoadInfo(CaminhoDoArquivoDeReplay);

        //Limpando as listas //TO DO: decidir se é uma boa salvas os pontos antes de apaga-los (ou em arquivo ou listas auxiliares)
        pontos.Clear();
        tempos.Clear();

        foreach (InfoperSecond informacao in infoHeatmap)
        {
            pontos.Add(informacao.heatmapPoint);
            tempos.Add(informacao.time);
        }
        //if (!updateEmTempoReal) { imprimirHeatmap(); }

    }



    protected IEnumerator replayTempoReal(){
        //TO DO: Verificar se limpar o heatmap (apagar, deletar) resolve os erros com o modo mudando durante a execucao
        //Pega do arquivo o tempo e ponto que sera impresso no momento correto
        List<Vector3> auxPontos = pontos.GetRange(indexPontos, 1);
        List<float>   auxTempos = tempos.GetRange(indexTempos, 1);
        
        Vector3 pontoAtual = Vector3.zero;
        float   tempoAtual = 0;

        foreach(Vector3 ponto in auxPontos) { pontoAtual = ponto; }
        foreach(float   tempo in auxTempos) { tempoAtual = tempo; }

        if (tempoAtual <= Time.time)
        {
            //Imprime esse ponto
            imprimirPontoIndividualmente(pontoAtual);

            //Atualiza index para proximo ponto
            indexPontos++;
            indexTempos++;
        }

        yield return null;   
    }


    //Atualiza a escala dos objetos de ponto de heatmap de acordo com uma escala do usuario
    private void mudarEscala(float escalaRelativaDosPontos)
    {
        for(int n=0; n<3; n++)
        {
            e_H_novo[n].transform.localScale   = escalaOriginal[n] * escalaRelativaDosPontos;
        }
    }

    private void mudarCorEOpacidade()
    {
        //e_H_novo.GetComponent<Texture3D>().SetPixel();
        //struct gradiente_alpha = objeto_opacidade.GetComponent<GradientAlphaKey>;
        //corNova = corOriginal;//CUIDADO AQUI, dependendo de como for implementado
        //A cor pode ser alterada por mudanças dos canais RGB, porem necessitariamos de 3tiposx3canais de opções de escolha
        //corNova[0].ToString//Verificar se funciona com o parametro vazio e o que tem de saida aqui
        Debug.Log("ENTROU PARA ALTERAR COR E OPACIDADE");

        for(int n=0; n<3; n++)
        {
            //corNova.r = ;
            corNova[n].a = opacidade;//Mudando o Alpha (Opacidade)
            _renderer[n].sharedMaterial.SetColor("novaCor",corNova[n]);
            //_renderer[n].material.SetColor("novaCor",corNova[n]);
        }

    }



}

