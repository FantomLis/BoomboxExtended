﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using FantomLis.BoomboxExtended.Containers;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Utils;

public class AlertUtils
{
    protected static List<MoneyCellAlertContainer> MoneyCellAlertQueue = new();
    public static void DropQueuedMoneyCellAlert(string header) => MoneyCellAlertQueue.RemoveAll(x => x.Header == header);
    public static void AddMoneyCellAlert(string header, MoneyCellUI.MoneyCellType type, string body, bool forceNow = false, bool dropQueuedAlert = false)
    {
        if (forceNow) 
        {
            ShowMoneyCellAlert(new MoneyCellAlertContainer(header, type, body));
            return;
        }
    
        var x = MoneyCellAlertQueue.Find(x => x.Header == header);
        if (x != null)
        {
            if (dropQueuedAlert) x.Description.Clear();
            x.Description.Add(body);
        }
        else {
            var alertContainer = new MoneyCellAlertContainer(header, type, body);
            MoneyCellAlertQueue.Add(alertContainer);
            Boombox.Self.StartCoroutine(DrawAllPendingMoneyCellAlerts(alertContainer));
        }
    }
    
    private static IEnumerator DrawAllPendingMoneyCellAlerts(MoneyCellAlertContainer a)
    {
        while (Boombox.Self)
        {
            yield return new WaitForSeconds(0.25f);
            if (!Player.localPlayer) continue;
            ShowMoneyCellAlert(a);
        }
    }
    
    private static void ShowMoneyCellAlert(MoneyCellAlertContainer a)
    {
        if (!Player.localPlayer) return;
        StringBuilder b = new();
        List<string> d = new();
        a.Description.ForEach(x => {
            for (int i = 0; i < x.Length; i+= 32)
            {
                d.Add(x.Substring(i,32));
            }
        });
        for (int i = 0; i < d.Count; i++)
        {
            if (i >= 1 && d.Count - i > 1)
            {
                b.Append($"... ({d.Count - i} more lines)");
                break;
            }
    
            b.Append(d[i] + "\n");
        }
        UserInterface.ShowMoneyNotification(a.Header, b.ToString(),
            a.AlertType);
    }
}