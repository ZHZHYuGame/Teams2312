using System.Collections;
using System.Collections.Generic;
using Data;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class AddToBagCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        BagProxy bagProxy = Facade.RetrieveProxy(BagProxy.bagModelProxy) as BagProxy;
        bagProxy?.AddToBag(notification.Body as GoodsData);
    }
}

public class BagUpdateCommand : SimpleCommand
{
    
}
