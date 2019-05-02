﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Seguradores/Segurador de Cartas")]
public class SeguradorDeCartas : ScriptableObject
{
    public VariavelTransform gridMao;
    public VariavelTransform gridCartasBaixadas;

    public void CarregarJogador(SeguradorDeJogador seguradorJogador)
    {
        foreach (InstanciaCarta c in seguradorJogador.cartasBaixadas)
        {
            Configuracoes.DefinirPaiCarta(c.infoCarta.gameObject.transform, gridCartasBaixadas.valor.transform);
        }
        foreach (InstanciaCarta c in seguradorJogador.cartasMao)
        {
            Configuracoes.DefinirPaiCarta(c.infoCarta.gameObject.transform, gridMao.valor.transform);
        }
    }
}