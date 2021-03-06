﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AdmJogo : MonoBehaviour
{
    public AudioClip hit, somBaixarCarta, somNaoPode, somCura;
    public float tempoAnimacaoCuraDano;
    public int numMaxCartasMao;
    public Baralho baralhoTutorial1, baralhoTutorial2, baralhoCompleto;
    public bool tutorial, inicioTutorial;
    public bool pause;
    public GameObject prefabCarta;//quando formos instanciar uma carta, precisamos saber qual é a carta, por isso passamos essa referencia

    public GameObject telaFimDeJogo;

    public GameObject telaPause;
    [HideInInspector]
    public int rodadaAtual;
    public EstadoJogador emSeuTurno;
    public Efeito efeitoAtual;
    public EstadoJogador estadoAtual;//variávei que nos diz qual é o estado atual do jogador atual
    public SeguradorDeJogador jogadorAtual;//variável que nos diz qual é o jogador atual.
    public SeguradorDeJogador jogadorLocal;
    public SeguradorDeJogador jogadorInimigo;
    public SeguradorDeJogador jogadorIA;

    public SeguradorDeCartas seguradorCartasJogadorAtual;
    public SeguradorDeCartas seguradorCartasJogadorInimigo;

    //definir no editor \/
    public InfoUIJogador infoJogadorAtual;
    public InfoUIJogador infoJogadorInimigo;
    [HideInInspector]
    public SeguradorDeJogador jogadorAtacado;
    [HideInInspector]
    public InstanciaCarta cartaAtacada;
    [HideInInspector]
    public InstanciaCarta cartaAtacante;
    [HideInInspector]
    public InstanciaCarta cartaAlvo;
    [HideInInspector]
    public SeguradorDeJogador jogadorAlvo;
    public GameEvent cartaMatou, jogadorPassouTurno, boiunaAtacouLobis, boitataAtacouJogador, turnoInimigoIA;
    public Image ImagemTextoTurno;
    public Sprite cursorAlvoCinza, cursorSegurandoCarta, cursorIdle, cursorAlvoVermelho, cursorAlvoVerde;
    public VariavelCarta cartaAtual;
    public GameEvent aoOlharCarta, aoPararDeOlharCarta;

    public static AdmJogo singleton;
    private void Awake()
    {
        singleton = this;
    }
    private void Start()
    {

        /*  
        A classe estática configurações vai possuir o admJogo como atributo,
        assim, nas configurações podemos mudar o admJogo também.
        */
        Configuracoes.admJogo = this;
        if (inicioTutorial)
        {
            jogadorLocal.baralhoInicial = baralhoTutorial1;
            jogadorInimigo.baralhoInicial = baralhoTutorial2;
            pause = true;
        }
        else
        {
            jogadorLocal.baralhoInicial = baralhoCompleto;
            jogadorInimigo.baralhoInicial = baralhoCompleto;
            pause = false;
        }
        GetComponent<AudioSource>().volume = Configuracoes.volumeSFX;
        jogadorLocal.InicializarJogador();
        jogadorIA.InicializarJogador();
        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Texto Passou").SetActive(false);
        DefinirEstado(emSeuTurno);
        if (tutorial == false)
            StartCoroutine(FadeTextoTurno(jogadorAtual));
    }

    private void Update()
    {
        if (!pause)
        {
            if (estadoAtual != null)
            {
                estadoAtual.Tick(Time.deltaTime);//percorre as ações do jogador naquele estado e permite que ele as execute
            }
        }
    }

    public void PuxarCarta(SeguradorDeJogador jogador)
    {
        if (jogador.cartasMao.Count < numMaxCartasMao)
        {
            AdmRecursos ar = Configuracoes.GetAdmRecursos();//precisamos acessar o admRecursos
            GameObject carta = Instantiate(prefabCarta) as GameObject;//instanciamos a carta de acordo com o prefab
            ExibirInfoCarta e = carta.GetComponent<ExibirInfoCarta>();//pegamos todas as informações atribuidas de texto e posição dela
            InstanciaCarta instCarta = carta.GetComponent<InstanciaCarta>();
            e.CarregarCarta(ar.obterInstanciaCarta(jogador.baralho.cartasBaralho[jogador.baralho.cartasBaralho.Count - 1]));//e por fim dizemos que os textos escritos serão os da carta na mão do jogador
            instCarta.carta = e.carta;
            instCarta.SetPoderECusto();
            e.CarregarCarta(instCarta.carta);
            instCarta.logicaAtual = jogador.logicaMao;//define a lógica pra ser a lógica da mão
            if (instCarta.carta.efeito != null)
            {
                Efeito novoEfeito = ScriptableObject.CreateInstance("Efeito") as Efeito;
                // novoEfeito = instCarta.carta.efeito;
                novoEfeito.name = instCarta.carta.efeito.name;
                novoEfeito.afetaApenasSeuJogador = instCarta.carta.efeito.afetaApenasSeuJogador;
                novoEfeito.afetaTodasCartas = instCarta.carta.efeito.afetaTodasCartas;
                novoEfeito.alteracaoMagia = instCarta.carta.efeito.alteracaoMagia;
                novoEfeito.alteracaoPoder = instCarta.carta.efeito.alteracaoPoder;
                novoEfeito.alteracaoVida = instCarta.carta.efeito.alteracaoVida;
                novoEfeito.apenasJogador = instCarta.carta.efeito.apenasJogador;
                novoEfeito.ativacao = instCarta.carta.efeito.ativacao;
                novoEfeito.cartaAlvo = instCarta.carta.efeito.cartaAlvo;
                novoEfeito.cartaQueInvoca = instCarta;
                novoEfeito.condicaoAtivacao = instCarta.carta.efeito.condicaoAtivacao;
                novoEfeito.escolheAlvoCarta = instCarta.carta.efeito.escolheAlvoCarta;
                novoEfeito.eventoAtivador = instCarta.carta.efeito.eventoAtivador;
                novoEfeito.jogadorAlvo = instCarta.carta.efeito.jogadorAlvo;
                novoEfeito.jogadorQueInvoca = jogador;
                novoEfeito.modoDeExecucao = instCarta.carta.efeito.modoDeExecucao;
                novoEfeito.podeUsarEmSi = instCarta.carta.efeito.podeUsarEmSi;
                novoEfeito.tipoEfeito = instCarta.carta.efeito.tipoEfeito;
                instCarta.efeito = novoEfeito;

                if (instCarta.efeito.apenasJogador)
                {
                    //afeta vc
                    if (instCarta.efeito.afetaApenasSeuJogador)
                    {
                        instCarta.efeito.jogadorAlvo = jogador;
                    }
                    else//afeta o inimigo
                    {
                        if (jogador == jogadorIA)
                        {
                            instCarta.efeito.jogadorAlvo = jogadorLocal;
                        }
                        else
                        {
                            instCarta.efeito.jogadorAlvo = jogadorIA;
                        }
                    }
                }
            }
            instCarta.jogadorDono = jogador;
            Configuracoes.DefinirPaiCarta(carta.transform, jogador.seguradorCartas.gridMao.valor);//joga as cartas fisicamente na mão do jogador
            jogador.cartasMao.Add(instCarta);
            jogador.baralho.cartasBaralho.RemoveAt(jogador.baralho.cartasBaralho.Count - 1);
            if (jogador == jogadorAtual)
            {
                carta.transform.Find("Fundo da Carta").gameObject.SetActive(false);
            }
            
            else
            {
                carta.transform.Find("Fundo da Carta").gameObject.SetActive(true);
            }
        }
    }
    IEnumerator FadeVencedorTurno(SeguradorDeJogador jogadorVencedorTurno)
    {
        aoPararDeOlharCarta.Raise();
        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().color = jogadorVencedorTurno.corJogador;
        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().text = jogadorVencedorTurno.nomeJogador + "\nVenceu a Rodada";
        ImagemTextoTurno.GetComponent<Image>().sprite = jogadorVencedorTurno.textoTurnoImage;
        ImagemTextoTurno.gameObject.SetActive(true);
        pause = true;
        yield return new WaitForSeconds(2);
        pause = false;
        ImagemTextoTurno.gameObject.SetActive(false);
        if (jogadorInimigo.barrasDeVida <= 0)
        {
            StartCoroutine(FimDeJogo(jogadorAtual));
        }
        if (jogadorAtual.barrasDeVida <= 0)
        {
            StartCoroutine(FimDeJogo(jogadorInimigo));
        }
        TrocarJogadorAtual();
        jogadorAtual.rodada.IniciarRodada();
        jogadorInimigo.rodada.IniciarRodada();
        if (jogadorAtual != jogadorVencedorTurno)
        {
            TrocarJogadorAtual();
        }
    }
    public void ChecaVidaJogadores()
    {
        if (jogadorAtual.vida <= 0 || jogadorInimigo.vida <= 0)
        {
            if (jogadorAtual.vida <= 0)
            {
                jogadorAtual.barrasDeVida--;
                StartCoroutine(FadeVencedorTurno(jogadorInimigo));
            }
            else if (jogadorInimigo.vida <= 0)
            {
                jogadorInimigo.barrasDeVida--;
                StartCoroutine(FadeVencedorTurno(jogadorAtual));
            }
            if (jogadorAtual.vida <= 0 && jogadorInimigo.vida <= 0)
            {
                StartCoroutine(Empate());
            }
            return;
        }
        if (jogadorAtual.passouRodada && jogadorInimigo.passouRodada)
        {
            if (jogadorAtual.vida > jogadorInimigo.vida)
            {
                jogadorInimigo.barrasDeVida--;

                StartCoroutine(FadeVencedorTurno(jogadorAtual));
            }
            else if (jogadorAtual.vida < jogadorInimigo.vida)
            {
                jogadorAtual.barrasDeVida--;
                StartCoroutine(FadeVencedorTurno(jogadorInimigo));
            }
            else
            {
                StartCoroutine(Empate());
            }
        }

    }
    public void TrocarJogadorAtual()
    {
        //se na hora da troca o jogador de baixo for o Player
        if (jogadorAtual == jogadorLocal)
        {
            jogadorAtual = jogadorInimigo;
            jogadorInimigo = jogadorLocal;
        }
        else
        {
            jogadorAtual = jogadorLocal;
            jogadorInimigo = jogadorIA;
        }
        jogadorAtual.seguradorCartas = seguradorCartasJogadorAtual;
        jogadorInimigo.seguradorCartas = seguradorCartasJogadorInimigo;
        seguradorCartasJogadorAtual.CarregarCartasJogador(jogadorAtual, infoJogadorAtual);
        seguradorCartasJogadorInimigo.CarregarCartasJogador(jogadorInimigo, infoJogadorInimigo);

        jogadorAtual.rodada.turno.IniciarTurno();
    }

    public void Pausar()
    {
        Configuracoes.admCursor.MudarSprite(cursorIdle);
        telaPause.gameObject.SetActive(true);
        pause = true;
    }
    public void Retomar()
    {
        if (estadoAtual == emSeuTurno)
        {
            Configuracoes.admCursor.MudarSprite(cursorIdle);
        }
        if (estadoAtual.name == "Atacando" || estadoAtual.name == "Usando Efeito")
        {
            Configuracoes.admCursor.MudarSprite(cursorAlvoCinza);
        }
        pause = false;
        StartCoroutine(RetomarMesmo());
    }
    IEnumerator RetomarMesmo()
    {
        yield return new WaitForSeconds(0.2f);
        telaPause.gameObject.SetActive(false);
    }
    public IEnumerator FimDeJogo(SeguradorDeJogador jogadorVencedor)
    {
        telaFimDeJogo.gameObject.SetActive(true);
        telaFimDeJogo.transform.Find("Retrato Jogador").GetComponent<Image>().sprite = jogadorVencedor.retratoJogador;
        telaFimDeJogo.transform.Find("Moldura").GetComponent<Image>().sprite = jogadorVencedor.moldura;
        yield return new WaitForSeconds(2);
        telaFimDeJogo.gameObject.SetActive(false);
        Pausar();
    }
    public void DefinirEstado(EstadoJogador estado)//função que altera o estado do jogador
    {
        estadoAtual = estado;
        Sprite spriteCursor = null;
        if (estado.name == "Em Seu Turno")
        {
            spriteCursor = cursorIdle;
            GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Cursor").GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        }
        if (estado.name == "Atacando" || estado.name == "Usando Efeito")
        {
            if (estado.name == "Atacando")
            {
                spriteCursor = cursorAlvoVermelho;
            }
            spriteCursor = cursorAlvoCinza;
            GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Cursor").GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        if (estado.name == "Segurando Carta")
        {
            spriteCursor = cursorSegurandoCarta;
            GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Cursor").GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        if (Configuracoes.admCursor != null)
            Configuracoes.admCursor.MudarSprite(spriteCursor);
    }
    public IEnumerator Empate()
    {
        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().color = new Color(255, 255, 255);
        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().text = "Empate";
        ImagemTextoTurno.GetComponent<Image>().sprite = jogadorAtual.textoTurnoImage;
        ImagemTextoTurno.gameObject.SetActive(true);
        pause = true;
        yield return new WaitForSeconds(1);
        pause = false;
        ImagemTextoTurno.gameObject.SetActive(false);
        jogadorAtual.barrasDeVida--;
        jogadorInimigo.barrasDeVida--;
        if (jogadorInimigo.barrasDeVida <= 0 && jogadorAtual.barrasDeVida <= 0)
        {
            Configuracoes.admCena.CarregarCena("Tela Inicial");
            yield return null;
        }
        if (jogadorInimigo.barrasDeVida <= 0)
        {
            StartCoroutine(FimDeJogo(jogadorAtual));
            yield return null;
        }
        else if (jogadorAtual.barrasDeVida <= 0)
        {
            StartCoroutine(FimDeJogo(jogadorInimigo));
            yield return null;
        }
        jogadorAtual.rodada.IniciarRodada();
        jogadorInimigo.rodada.IniciarRodada();
        TrocarJogadorAtual();
    }

    public void TocarSomCartaBaixada()
    {
        if (GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = somBaixarCarta;
        GetComponent<AudioSource>().Play();
    }
    public void TocarSomNaoPode()
    {
        if (GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = somNaoPode;
        GetComponent<AudioSource>().Play();
    }
    public void TocarSomDano()
    {
        if (GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = hit;
        GetComponent<AudioSource>().Play();
    }
    public void TocarSomCura()
    {
        if (GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = somCura;
        GetComponent<AudioSource>().Play();
    }
    public IEnumerator FadeTextoTurno(SeguradorDeJogador jogador)
    {
        aoPararDeOlharCarta.Raise();
        if (jogador.passouRodada == false)
        {
            GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().color = jogador.corJogador;
            GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Fundo turno/Turno").GetComponent<Text>().text = "Turno de\n" + jogador.nomeJogador;
            ImagemTextoTurno.GetComponent<Image>().sprite = jogador.textoTurnoImage;
            ImagemTextoTurno.gameObject.SetActive(true);
            pause = true;
            yield return new WaitForSeconds(1);
            pause = false;
            ImagemTextoTurno.gameObject.SetActive(false);
        }
    }
    public void EncerrarRodada()
    {
        if (pause == true)
        {
            return;
        }
        aoPararDeOlharCarta.Raise();
        jogadorAtual.rodada.turno.FinalizarTurno();
        if (jogadorAtual.fezAlgumaAcao)//somente passou o turno
        {
            if (jogadorInimigo.passouRodada == false)
            {
                if (tutorial == false)
                {
                    TrocarJogadorAtual();
                    jogadorAtual.rodada.turno.IniciarTurno();

                }
                else
                {
                    foreach (InstanciaCarta c in jogadorInimigo.cartasBaixadas)
                    {
                        c.protegido = false;
                        c.podeSofrerEfeito = true;
                        c.podeSerAtacada = true;
                        c.infoCarta.protegido = false;
                        c.infoCarta.CarregarCarta(c.infoCarta.carta);
                    }
                    if (jogadorAtual == jogadorLocal)
                    {
                        Configuracoes.turnoDaIATutorial = true;
                        jogadorPassouTurno.Raise();
                        turnoInimigoIA.Raise();
                    }
                }
            }
            else
            {
                jogadorAtual.rodada.turno.IniciarTurno();
            }
        }
        else//pretende encerrar a rodada
        {
            jogadorAtual.rodada.PassarRodada();
            if (jogadorInimigo.passouRodada && jogadorAtual.passouRodada)
            {
                ChecaVidaJogadores();
            }
            else
            {
                if (tutorial)
                {
                    ChecaVidaJogadores();
                    jogadorPassouTurno.Raise();
                    jogadorAtual.rodada.turno.IniciarTurno();
                    return;
                }
                TrocarJogadorAtual();
                jogadorAtual.rodada.turno.IniciarTurno();
            }
        }
    }
    public IEnumerator Atacar()
    {
        //Atacar uma carta
        if (cartaAtacada != null && cartaAtacante != null)
        {
            cartaAtacante.gameObject.transform.localScale = new Vector3(0.28f, 0.28f, 1);
            int poderCartaAtacanteAntes = cartaAtacante.poder;
            int poderCartaAtacadaAntes = cartaAtacada.poder;
            StartCoroutine(cartaAtacada.AnimacaoDano(poderCartaAtacanteAntes * -1));
            StartCoroutine(cartaAtacante.AnimacaoDano(poderCartaAtacadaAntes * -1));
            yield return new WaitForSeconds(tempoAnimacaoCuraDano);
            cartaAtacada.poder -= poderCartaAtacanteAntes;
            cartaAtacante.poder -= poderCartaAtacadaAntes;
            int poderCartaAtacanteDepois = cartaAtacante.poder;
            int poderCartaAtacadaDepois = cartaAtacada.poder;

            if (poderCartaAtacadaDepois <= 0)
            {
                cartaMatou.cartaQueAtivouEvento = cartaAtacante;
                Configuracoes.admEfeito.eventoAtivador = cartaMatou;
                cartaMatou.Raise();
                MatarCarta(cartaAtacada, cartaAtacada.jogadorDono);
            }
            if (poderCartaAtacanteDepois <= 0)
            {
                MatarCarta(cartaAtacante, cartaAtacante.jogadorDono);
            }
            cartaAtacante.infoCarta.CarregarCarta(cartaAtacante.infoCarta.carta);
            cartaAtacada.infoCarta.CarregarCarta(cartaAtacada.infoCarta.carta);
            if (tutorial && cartaAtacante.carta.name == "Boiuna" && cartaAtacada.carta.name == "Lobisomem")
            {
                boiunaAtacouLobis.Raise();
            }
            cartaAtacante.podeAtacarNesteTurno = false;
            cartaAtacada = null;
            cartaAtacante = null;
        }
        //Atacar um jogador
        if (cartaAtacante != null && jogadorAtacado != null)
        {
            cartaAtacante.gameObject.transform.localScale = new Vector3(0.28f, 0.28f, 1);
            int poderCartaAtacanteAntes = cartaAtacante.poder;
            StartCoroutine(jogadorAtacado.infoUI.AnimacaoDano(poderCartaAtacanteAntes * -1));
            StartCoroutine(cartaAtacante.AnimacaoDano(-1));
            yield return new WaitForSeconds(tempoAnimacaoCuraDano);
            jogadorAtacado.vida -= poderCartaAtacanteAntes;
            cartaAtacante.poder--;
            jogadorAtacado.infoUI.AtualizarVida();
            cartaAtacante.infoCarta.CarregarCarta(cartaAtacante.infoCarta.carta);
            if (cartaAtacante.poder <= 0)
            {
                MatarCarta(cartaAtacante, cartaAtacante.jogadorDono);
            }
            if (tutorial && cartaAtacante.carta.name == "Boitatá")
            {
                boitataAtacouJogador.Raise();
            }
            cartaAtacante.podeAtacarNesteTurno = false;
            cartaAtacante = null;
            jogadorAtacado = null;
            ChecaVidaJogadores();
        }
        yield return null;
    }
    public void MatarCarta(InstanciaCarta c, SeguradorDeJogador jogador)
    {
        if (jogador.cartasBaixadas.Count == 0)
        {
            return;
        }
        foreach (InstanciaCarta carta in jogador.cartasBaixadas)
        {
            if (jogador.cartasBaixadas.Contains(c))
            {
                c.gameObject.SetActive(false);
                jogador.ColocarCartaNoCemiterio(c);
                jogador.cartasBaixadas.Remove(c);
                break;
            }
        }
    }
    public IEnumerator DestacarCartaBaixada(InstanciaCarta instCarta)
    {
        // fechando outras cartas em destaque   
        aoPararDeOlharCarta.Raise();

        cartaAtual.Set(instCarta);

        GameObject.Find("/Screen Overlay Canvas/Interface do Usuário/Carta Sendo Olhada/Carta sendo olhada").SetActive(true);
        Configuracoes.cartaRecemJogada = true;
        aoOlharCarta.Raise();
        yield return new WaitForSeconds(1.5f);
        Configuracoes.cartaRecemJogada = false;
        aoPararDeOlharCarta.Raise();
    }
}