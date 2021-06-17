using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ViveSR.anipal.Eye;
using Tobii.XR;

namespace HeatmapLegacy{
//P.S. Os nomes de variaveis e metodos podem estar confusos> estao misturados termos em pt-br e eng.
public class HeatmapLegacy : MonoBehaviour
{
    
    public bool EmExecucao = true;
    public bool showHeatmap = false;
    public bool updateEmTempoReal;
    public bool somenteRastro;
    public int tamanhoDoRastro=10;
    public float tempoParaUpdate;
    public float distanciaMaxima = 0.7f;
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
    public bool aplicarCor = false;
    public float opacidade;


    //Camera principal ou HMD
    private bool HMD = false;
    private Camera cam;

    [SerializeField]
    //private RaycastHit pontoAtual, pontoAnterior;
    private Vector3 pontoAnterior = new Vector3(0,0,0);

    //Variaveis para a personalizacao dos pontos (escala, cor, opacidade) 
    private GameObject[] e_H_novo = new GameObject[3]; //efeito_Heatmap_novo
    private float escalaRelativaDosPontos_anterior = 1;
    private Vector3[] escalaOriginal = new Vector3[3];
    private Renderer[] _renderer = new Renderer[3];
    private Color[] corOriginal = new Color[3];
    //private int[] hash_corOriginal = new int[3];
    private float opacidadeOriginal, opacidadeAnterior;



    //Armazenando ObjetoParent, listas de pontos e seus objetos e uma fila para o rastro.
    protected List<Vector3> pontos = new List<Vector3>();
    protected List<GameObject> objetosHeatmap = new List<GameObject>();
    //protected List<GameObject> colliders = new List<GameObject>(); //Para encontrar objetos que foram destruidos por pouca utilidade
    protected Queue<GameObject> rastro = new Queue<GameObject>();
    protected Vector3[] dwell_points = new Vector3[10];



    //Usando o BinaryManager para guardar os dados
    private ItemEntry user;
    private List<InfoperSecondHeatmap> infoHeatmap = new List<InfoperSecondHeatmap>();
    protected List<float> tempos = new List<float>();//Para sincronizar tempo do player com o arquivo de replay
    //Auxiliares para replay
    protected int indexPontos=0 , indexTempos=0;



    private void Start()
    {
        objetoPai = gameObject; //Associa o objeto que contem o script
        
    //Criando objetos de heatmap modificaveis no contexto
        //Vector3 pontoDistante = new Vector3(-100,-100,-100);
        //Vector3 pontoDistante = Vector3.zero;
        ////Cria um objeto base para outros
        //e_H_novo[0] = efeitoHeatmap;
        //Instantiate(efeitoHeatmap, pontoDistante,Quaternion.identity);

        // e_H_novo[0] = GameObject.Instantiate(efeitoHeatmap        ,pontoDistante, Quaternion.identity, objetoPai.transform) as GameObject; 
        // e_H_novo[1] = GameObject.Instantiate(efeitoHeatmapLaranja ,pontoDistante, Quaternion.identity, objetoPai.transform) as GameObject; 
        // e_H_novo[2] = GameObject.Instantiate(efeitoHeatmapVermelho,pontoDistante, Quaternion.identity, objetoPai.transform) as GameObject; 
        
        //Não serve para alterar cor, mas não é necessario instanciar pontos visíveis no inicio
        e_H_novo[0] = efeitoHeatmap; 
        e_H_novo[1] = efeitoHeatmapLaranja;
        e_H_novo[2] = efeitoHeatmapVermelho;
        
       
        //Escala (testar for loop)
        // escalaOriginal[0] = efeitoHeatmap.transform.localScale;
        // escalaOriginal[1] = efeitoHeatmapLaranja.transform.localScale;
        // escalaOriginal[2] = efeitoHeatmapVermelho.transform.localScale;

        //Escala, cor e opacidade do Heatmap (tres tipos)
        for(int n=0; n<3; n++)
        {
            escalaOriginal[n] = e_H_novo[n].transform.localScale;
            _renderer[n] = e_H_novo[n].GetComponent<Renderer>(); 
            //corOriginal[n] = _renderer[n].sharedMaterial.color; //Referncia mesma cor, perde valores originais com modificacoes

            float r, g, b, a;
            r = _renderer[n].sharedMaterial.color.r;
            g = _renderer[n].sharedMaterial.color.g;
            b = _renderer[n].sharedMaterial.color.b;
            a = _renderer[n].sharedMaterial.color.a;
            corOriginal[n] = new Color(r,g,b,a);

            if(aplicarCor){} //Novas cores ja escolhidas
            else corNova[n] = new Color(r,g,b,a);
            
        }
       
        
        //Opacidade
        opacidadeOriginal = corOriginal[0].a;
        if(opacidade==0) opacidade = opacidadeOriginal; //Assumindo que é por falta de entrada, para evitar apagar tudo com mudança de cor
        opacidadeAnterior = opacidade;

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
            //Co-rotina que aguarda um certo periodo de tempo e captura o ponto disponível (naive)
            //StartCoroutine(storePoints());

            //Rotina que analisa a uma taxa fixa os ultimos pontos para determinar fixação
            StartCoroutine(dwellPoints());

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
        if (opacidade!=opacidadeAnterior || aplicarCor)
        {
            opacidadeAnterior = opacidade;
            mudarCorEOpacidade();    
            aplicarCor=false;        
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
        //Retornando opacidade e cor
        for(int n=0; n<3; n++){ _renderer[n].sharedMaterial.color = corOriginal[n];}

    }

    public IEnumerator dwellPoints()
    {
        //Pausa novas chamadas dessa rotina diretamente em Update()
        EmExecucao_background = false;

        Vector3 somaPontos = new Vector3(0,0,0);
        Vector3 pontoAtual;

        for(int n=0; n<10; n++)
        {
            //Escolha da camera
            if (!HMD) //Caso nao esteja funcionando o headset
            {
                cam = Camera.main;//Camera prinicipal da cena
                pontoAtual = capturaPonto(cam);//Captura do ponto de visao projetado no mundo
            }
            else pontoAtual = capturaPontoHMD();//para o HMD
            
            dwell_points[n] = pontoAtual;
            somaPontos += pontoAtual;
            //Debug.Log("Novo ponto achado");
            //Espera para a próxima captura
            yield return new WaitForSecondsRealtime(tempoParaUpdate/10);
        }

        Vector3 mediaPontos = somaPontos/10;
        Vector3 pontoFinal = new Vector3(0,0,0);
        bool proximos = true;
        //float distanciaMaxima = 0.5f;
        float menorDistancia = distanciaMaxima;
        float distanciaAtual;

        //Verifica a proximidade de todos os pontos, determinando se são proximos e qual deve ser o ponto final
        for(int n=0; n<10 && proximos ; n++)
        {
            distanciaAtual = Vector3.Distance(mediaPontos, dwell_points[n]);
            if(distanciaAtual < menorDistancia){
                menorDistancia = distanciaAtual;
                pontoFinal = dwell_points[n];
            }
            else if(distanciaAtual > distanciaMaxima){
                    proximos = false;
                    pontoFinal = Vector3.zero;
                 }
        }

        //Considerando que todos estão dentro do limite de distancia do ponto média podemos adiciona-los
        if(proximos){
            pontoAtual = pontoFinal;
            storePoints(pontoAnterior, pontoAtual);
            //Debug.Log("Novo ponto criado");
            pontoAnterior = pontoAtual;
        }


        //retoma a execução dessa rotina em Update()
        EmExecucao_background = true;
    }

    //Aquisição dos pontos de visao e armazenamento desses - com eventual impressao individual
    public void storePoints(Vector3 pontoAnterior, Vector3 pontoAtual)
    {
        // EmExecucao_background = false;//paralisa o update e continua o processo ao longo dos frames
        // //Debug.Log("Entrou em execucao");
        //     //TO DO: dwell-time
        // //Espera um periodo predeterminado para executar novamente a captura
        // yield return new WaitForSecondsRealtime(tempoParaUpdate);
        // //Debug.Log("deve ter esperado o tempo até update");

        // //Escolha da camera
        // if (!HMD) //Caso nao esteja funcionando o headset
        // {
        //     cam = Camera.main;//Camera prinicipal da cena
        //     //Captura do ponto de visao projetado no mundo
        //     pontoAtual = capturaPonto(cam);
        // }
        // else //para o HMD
        // {
        //     pontoAtual = capturaPontoHMD();
        // }


        if (pontoAnterior == pontoAtual) { }//RaycastHit.ReferenceEquals(pontoAnterior,pontoAtual)) { }
        else//se existir ponto ele sera armazenado
        {
            //Debug.Log("Ponto atual: " + pontoAtual.point);
            //Debug.Log("Ponto anterior: " + pontoAnterior.point);
            
            //Armazena pontos em formato Vector3 para construir o heatmap
            pontos.Add(pontoAtual);
            //pontoAnterior = pontoAtual;

            //Rastro e atualizacao em tempo real
            if (somenteRastro)
            {
                if (!(tamanhoDoRastro > rastro.Count))//Se não houver mais espaço na fila
                {
                    //Apagar o ponto que foi dequeued
                    GameObject.Destroy(rastro.Dequeue());
                    //Coloca o novo ponto com enqueue
                    rastro.Enqueue(imprimirPontoSimples(pontoAtual));
                }
                else //Aumenta a fila com o novo ponto
                {
                    rastro.Enqueue(imprimirPontoSimples(pontoAtual));
                }
            }
            else if(updateEmTempoReal)//Para impressao em tempo real ativa
            {
                imprimirPontoIndividualmente(pontoAtual);
            }
         


            //Salvar://Lista de InfoperSecondHeatmap para o ItemEntry do BinaryManager //Atualmente com as infos de EyeT zeradas
            infoHeatmap.Add(new InfoperSecondHeatmap(Time.time, pontoAtual,
                                        Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero));
        }


        EmExecucao_background = true;//Volta a ativacao pela rotina de update
    }


    private Vector3 capturaPonto(Camera cam)
    {
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));//Deprecated?
        RaycastHit pontoNoMundo;
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))//Distância arbitraria demais?
        {
            return pontoNoMundo.point;
        }
        else return pontoAnterior;
    }

    private Vector3 capturaPontoHMD()
    {
        var gazeRay = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay;
        if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.IsValid)//Verfica a validade dos dados. Redundante com o raycast?
        {

            Ray raio = new Ray(TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.Origin,
                                    TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).GazeRay.Direction);
            RaycastHit pontoNoMundo;
            if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))
            {
                return pontoNoMundo.point;
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

        HeatmapLegacyBinaryManager.SaveInfo(user, endereco);
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
        infoHeatmap = HeatmapLegacyBinaryManager.LoadInfo(CaminhoDoArquivoDeReplay);

        //Limpando as listas //TO DO: decidir se é uma boa salvas os pontos antes de apaga-los (ou em arquivo ou listas auxiliares)
        pontos.Clear();
        tempos.Clear();

        foreach (InfoperSecondHeatmap informacao in infoHeatmap)
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
        for(int n=0; n<3; n++)
        {
            corNova[n].a = opacidade;//Mudando o Alpha (Opacidade)
            _renderer[n].sharedMaterial.color = corNova[n];//Muda a cor diretamente no material
        }
    }



}

}