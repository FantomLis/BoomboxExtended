using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FantomLis.BoomboxExtended.Containers;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Utils;

public class AlertUtils
{
    private static List<MoneyCellAlertContainer> MoneyCellAlertQueue = new();
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
        Boombox.Self.StartCoroutine(DecayPendingMCAlert(a));
        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => Player.localPlayer);
        if (!MoneyCellAlertQueue.Contains(a)) yield break;
        ShowMoneyCellAlert(a);
        MoneyCellAlertQueue.Remove(a);
    }

    private static IEnumerator DecayPendingMCAlert(MoneyCellAlertContainer a)
    {
        yield return new WaitForSeconds(2.5f);
        if (MoneyCellAlertQueue.Contains(a)) MoneyCellAlertQueue.Remove(a);
    }
    
    private static void ShowMoneyCellAlert(MoneyCellAlertContainer a)
    {
        if (!Player.localPlayer) return;
        StringBuilder b = new();
        List<string> d = new();
        a.Description.ForEach(x => {
            for (int i = 0; i < x.Length; i+= 32)
            {
                d.Add(x.Substring(i,Math.Min(32, x.Length-i)));
            }
        });
        for (int i = 0; i < d.Count; i++)
        {
            if (i >= 1 && d.Count - i > 1)
            {
                b.Append(d[i] +$"...");
                break;
            }
    
            b.Append(d[i] + "\n");
        }
        UserInterface.ShowMoneyNotification(a.Header, b.ToString(),
            a.AlertType);
    }
}