//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//P.S. Os nomes de variaveis e metodos podem estar confusos> estao misturados termos em pt-br e eng.
public class StoreViewpoints : MonoBehaviour
{
    public bool EmExecucao = true;
    public bool updateEmTempoReal;
    public float tempoParaUpdate;
    public GameObject efeitoHeatmap;
    public GameObject objetoPai;

    //Para visualizacao de gravacoes anteriores
    public bool ModoPlayer = false;
    public string CaminhoDoArquivoDeReplay;


    private Camera cam;

    [SerializeField]
    private RaycastHit pontoAtual, pontoAnterior;
    private bool ativacaoHeatmap = false;



    //Armazenando pontos em uma lista privada.
    private List<Vector3> pontos = new List<Vector3>();
    //Armazenando uma lista de objetos de heatmap
    private List<GameObject> objetosHeatmap = new List<GameObject>();

    //Usando o BinaryManager para guardar os dados
    private ItemEntry user;
    private List<InfoperSecond> infoHeatmap = new List<InfoperSecond>();
    private List<float> tempos = new List<float>();//Para sincronizar tempo do player com o arquivo de replay
    //Auxiliares para replay
    private int indexPontos=0 , indexTempos=0;

    private void Start()
    {
        //efeitoHeatmap = GameObject.CreatePrimitive(PrimitiveType.Sphere);//para objeto privado

        //Para modo de replay -> Lê o arquivo e faz aquisicao dos dados salvos
        if (ModoPlayer) {
            EmExecucao = false;
            Debug.Log("Modo Player ativo - Novas entradas de Heatmap não são aceitas, construido a partir do arquivo fornecido");
            playerHeatmapArquivo();
        }
    }

    // Update é chamado a cada frame, mas se respeita o tempo para execucao
    void Update()
    {
        if (EmExecucao)
        {
            StartCoroutine(storePoints());
        }
        else if (ModoPlayer && updateEmTempoReal) { replayTempoReal(); }//Para modo replay em tempo real

        //Tecla H para ativar/desativar a visualizacao do heatmap
        if (Input.GetKeyDown(KeyCode.H))
        {
            //Alterna o estado de ativacao com o pressionar da tecla
            ativacaoHeatmap = !ativacaoHeatmap;
            //Caso desejado, chama o metodo de impressao do heatmap
            if (ativacaoHeatmap) { imprimirHeatmap(); }
            else { apagarHeatmap(); }
        }
        //Pode ser criado metodo especifico para imprimir novas entradas diretamente caso ativacaoHeatmap=true;

        //Metodo para salvar o arquivo imediatamente, pela tecla "j"
        if (Input.GetKeyDown(KeyCode.J)) { gravarDadosCompletos(); Debug.Log("Pontos armazenados salvos"); }

    }



    //Salva os dados com o BinaryManager ao sair da aplicacao
    void OnApplicationQuit()
    {
        if (!ModoPlayer) { //Somente no modo normal, o modo replay nao deve armazenar novamente os dados
            BinaryManagerSave();
            Debug.Log("Aplicacao parada e dados salvos");
        }
    }



    //Aquisição dos pontos de visao e armazenamento desses - com eventual impressao individual
    public IEnumerator storePoints()
    {
        EmExecucao = false;//paralisa o update e continua o processo ao longo dos frames
        Debug.Log("Entrou em execucao");
        yield return new WaitForSecondsRealtime(tempoParaUpdate);//Espera um periodo predeterminado para executar novamente a captura
        Debug.Log("deve ter esperado o tempo até update");

        //Escolha da camera
        cam = Camera.main;//Camera prinicipal da cena

        //Captura do ponto de visao projetado no mundo
        pontoAtual = capturaPonto(cam);

        if (pontoAnterior.point == pontoAtual.point) { }//RaycastHit.ReferenceEquals(pontoAnterior,pontoAtual)) { }
        else//se existir ponto ele sera armazenado
        {
            Debug.Log("Ponto atual: " + pontoAtual.point);
            Debug.Log("Ponto anterior: " + pontoAnterior.point);
            //Armazena pontos em formato Vector3 para construir o heatmap
            pontos.Add(pontoAtual.point);
            pontoAnterior = pontoAtual;

            //Para impressao em tempo real ativa -codigo copiado de impressaoHeatmap()
            if (updateEmTempoReal)
            {
                objetosHeatmap.Add(Instantiate(efeitoHeatmap, pontoAtual.point, Quaternion.identity, objetoPai.transform));
            }
            //Salvar://Lista de InfoperSecond para o ItemEntry do BinaryManager //Atualmente com as infos de EyeT zeradas
            infoHeatmap.Add(new InfoperSecond(Time.time, pontoAtual.point,
                                        Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero));
        }


        EmExecucao = true;//Volta a ativacao pela rotina de update
    }


    private RaycastHit capturaPonto(Camera cam)
    {
        Ray raio = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 pontoVistoPelaCamera = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, cam.nearClipPlane));
        RaycastHit pontoNoMundo;
        if (Physics.Raycast(raio, out pontoNoMundo, 100.0f))
        {
            return pontoNoMundo;
        }

        return pontoAnterior;
    }


    private void imprimirHeatmap()
    {
        //Limpando os objetos ja impressos
        apagarHeatmap();

        //Inicialmente so' imprime os pontos armazenados, sem considerar proximidade e temperatura
        foreach (Vector3 ponto in pontos) {
            //Lista de objetos e criacao 
            objetosHeatmap.Add(Instantiate(efeitoHeatmap, ponto, Quaternion.identity, objetoPai.transform));

        }

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
        foreach (InfoperSecond informacao in infoHeatmap)
        {
            pontos.Add(informacao.heatmapPoint);
            tempos.Add(informacao.time);
        }
        if (!updateEmTempoReal) { imprimirHeatmap(); }

    }

    private void replayTempoReal(){
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
            objetosHeatmap.Add(Instantiate(efeitoHeatmap, pontoAtual, Quaternion.identity, objetoPai.transform));

            //Atualiza index para proximo ponto
            indexPontos++;
            indexTempos++;
        }

        return;
               
    }





}

