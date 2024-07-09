
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

#pragma warning disable CS0612 // Type or member is obsolete

namespace Autodesk.Windows.Extensions
{
   public abstract class ModalCommandHandler : ICommand
   {
      public event EventHandler CanExecuteChanged;

      public ModalCommandHandler()
      {
         CanExecuteManager.Enabled = true;
         IsModal = true;
      }

      public virtual bool IsModal { get; set; }   

      /// <summary>
      /// Enables the command only when there is a quiescent document:
      /// </summary>
      
      public virtual bool CanExecute(object parameter)
      {
         return IsModal ? CanExecuteManager.HasQuiescentDocument : true;
      }

      public abstract void Execute(object parameter);

      protected static Editor Editor =>
         Application.DocumentManager.MdiActiveDocument?.Editor;
   }

}