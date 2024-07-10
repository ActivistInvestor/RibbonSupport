
/// ModalCommandHandler.cs
/// 
/// ActivistInvestor / Tony T
/// 
/// Distributed under the terms of the MIT license


using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Extensions;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Ribbon.Extensions;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Autodesk.Windows.Extensions
{
   public abstract class ModalCommandHandler : ICommand
   {
      public event EventHandler CanExecuteChanged;

      public ModalCommandHandler()
      {
         RibbonEventManager.QueryCanExecuteEnabled = true;
         IsModal = true;
      }

      public virtual bool IsModal { get; set; }   

      /// <summary>
      /// If IsModal is true, this enables the command only 
      /// when there is an active document that is quiescent:
      /// </summary>
      
      public virtual bool CanExecute(object parameter)
      {
         bool result = IsModal ? RibbonEventManager.IsQuiescentDocument : true;
         //if(QueryCanExecute != null)
         //{
         //   var args = new QueryCanExecuteEventArgs(result, parameter);
         //   QueryCanExecute?.Invoke(this, args);
         //   return args.CanExecute;
         //}
         return result;          
      }

      //public event QueryCanExecuteEventHandler QueryCanExecute;

      public abstract void Execute(object parameter);

      protected static Editor Editor =>
         Application.DocumentManager.MdiActiveDocument?.Editor;
   }

   public delegate void QueryCanExecuteEventHandler(object sender, QueryCanExecuteEventArgs e);

   public class QueryCanExecuteEventArgs : EventArgs
   {
      public QueryCanExecuteEventArgs(bool canExecute, object parameter)
      {
         this.CanExecute = canExecute;
         this.Parameter = parameter;
      }

      public bool CanExecute { get; set; }
      public object Parameter { get; private set; }
   }
}