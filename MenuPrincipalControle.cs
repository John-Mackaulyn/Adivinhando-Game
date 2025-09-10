using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuPrincipalControle : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Referências UI")]
    [SerializeField] private Button btnJogarSemLogin;
    [SerializeField] private Button btnJogarComLogin;
    [SerializeField] private Button btnSair;
    [SerializeField] private Button btnSom;
    [SerializeField] private Image imgSom;
    [SerializeField] private Sprite somLigadoSprite;
    [SerializeField] private Sprite somDesligadoSprite;
    [SerializeField] private GameObject painelLogin;
    [SerializeField] private GameObject painelCompra;

    [Header("Configurações")]
    [SerializeField] private float tempoFeedback = 1.5f;
    #endregion

    #region Métodos do Ciclo de Vida
    private void Start()
    {
        VerificarReferencias();
        ConfigurarBotoesUI();
        AtualizarIconeSom();
    }

    private void OnDestroy()
    {
        btnJogarSemLogin.onClick.RemoveAllListeners();
        btnJogarComLogin.onClick.RemoveAllListeners();
        btnSair.onClick.RemoveAllListeners();
        btnSom.onClick.RemoveAllListeners();
    }
    #endregion

    #region Configuração da UI
    // Configura os listeners dos botões e estado inicial dos painéis
    private void ConfigurarBotoesUI()
    {
        btnJogarSemLogin.onClick.AddListener(JogarSemLogin);
        btnJogarComLogin.onClick.AddListener(TentarLoginGoogle);
        btnSair.onClick.AddListener(SairDoJogo);
        btnSom.onClick.AddListener(AlternarSom);

        painelLogin.SetActive(false);
        painelCompra.SetActive(false);
    }

    // Verifica se as referências UI estão atribuídas
    private void VerificarReferencias()
    {
        if (btnJogarSemLogin == null) Debug.LogError("btnJogarSemLogin não atribuído!", this);
        if (btnJogarComLogin == null) Debug.LogError("btnJogarComLogin não atribuído!", this);
        if (btnSair == null) Debug.LogError("btnSair não atribuído!", this);
        if (btnSom == null) Debug.LogError("btnSom não atribuído!", this);
        if (imgSom == null) Debug.LogError("imgSom não atribuído!", this);
    }

    // Atualiza o ícone de som com base no estado
    private void AtualizarIconeSom()
    {
        imgSom.sprite = AudioListener.volume > 0 ? somLigadoSprite : somDesligadoSprite;
    }
    #endregion

    #region Ações da UI
    // Inicia o jogo sem login
    private void JogarSemLogin()
    {
        GerenciadorJogo.Instance.RealizarLogin(false);
        GerenciadorJogo.Instance.IrParaCena("Categorias");
    }

    // Inicia o processo de login
    private void TentarLoginGoogle()
    {
        painelLogin.SetActive(true);
        StartCoroutine(ProcessarLogin());
    }

    // Simula o processo de login
    private IEnumerator ProcessarLogin()
    {
        yield return new WaitForSeconds(tempoFeedback);

        bool loginSucesso = true;
        GerenciadorJogo.Instance.RealizarLogin(loginSucesso);

        painelLogin.SetActive(false);
        if (loginSucesso)
        {
            MostrarOpcoesPosLogin();
        }
    }

    // Exibe opções após login bem-sucedido
    private void MostrarOpcoesPosLogin()
    {
        if (!GerenciadorJogo.Instance.compraRealizada)
        {
            painelCompra.SetActive(true);
        }
        else
        {
            GerenciadorJogo.Instance.IrParaCena("Categorias");
        }
    }

    // Inicia o processo de compra
    public void TentarComprar()
    {
        StartCoroutine(ProcessarCompra());
    }

    // Simula o processo de compra
    private IEnumerator ProcessarCompra()
    {
        yield return new WaitForSeconds(tempoFeedback);

        bool compraSucesso = true;
        GerenciadorJogo.Instance.RealizarCompra(compraSucesso);

        painelCompra.SetActive(false);
        if (compraSucesso)
        {
            GerenciadorJogo.Instance.IrParaCena("Categorias");
        }
    }

    // Alterna o estado do som
    private void AlternarSom()
    {
        GerenciadorJogo.Instance.MutarSom();
        AtualizarIconeSom();
    }

    // Sai do jogo
    private void SairDoJogo()
    {
        GerenciadorJogo.Instance.SairDoJogo();
    }
    #endregion
}