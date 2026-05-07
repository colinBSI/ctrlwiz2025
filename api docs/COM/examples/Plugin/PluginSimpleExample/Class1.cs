//------------------------------------------------------------------
// Navisworks Sample code
//------------------------------------------------------------------
//
// (C) Copyright 2011 by Autodesk Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//------------------------------------------------------------------
// Navisworks API - PluginSimpleExample
// This is a simple example showing how to add an item onto the
// context menu. It also shows how to read properties associated with
// selected items.
//------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;


namespace PluginSimpleExample
{
   // Identify plugin with a suitable Guid and ProgId
   [ComVisible(true)]
   [Guid("C1DD99E9-1968-4092-8CDE-F7E196AC1124")]
   [ProgId("ADSK.MyCSharpUIPlugin")]

   // Plugin should be derived from "both" InwPlugin and InwExportPlugin
   public class MyCSharpUIPlugin : NavisworksIntegratedAPI22.InwPlugin, 
                                   NavisworksIntegratedAPI22.InwExportPlugin
   {
      private NavisworksIntegratedAPI22.InwPlugin_Site m_plugin_site;
      private Process m_process;


    //
    // InwPlugin methods
    //

      public virtual void AdviseSite(NavisworksIntegratedAPI22.InwPlugin_Site plugin_site)
      {
         m_plugin_site = plugin_site;
      }
      public virtual bool iActivate()
      {
         return true;
      }
      public virtual void iAppInitialising()
      {;}
      public virtual void iAppTerminating()
      {;}
      public virtual bool iDeactivate()
      {
         return true;
      }
      public virtual void iDoCustomOption()
      {;}
      public virtual String iGetDisplayName()
      {
         return "MyCSharpUIPlugin";
      }
      public virtual int iGetNumParameters()
      {
         return 0;
      }
      public virtual String iGetParameter(int ndx, ref object pData)
      {
         return null;
      }
      public virtual bool iSetParameter(int ndx, object newVal)
      {
         return false;
      }
      public virtual object iXtension(object vIn)
      {
         return null;
      }
      public virtual string ObjectName 
      {
         get { return "MyPluginObject"; }
      }
    //
    // InwExportPlugin methods
    //
      public virtual void iBeginContext(NavisworksIntegratedAPI22.InwOpState p_state)
      {
      }
      public virtual bool iCanExport(NavisworksIntegratedAPI22.InwOpState p_state, int reason)
      {
         return true;
      }
      public virtual bool iDialog(NavisworksIntegratedAPI22.InwOpState p_state, int reason)
      {
         return true;
      }
      public virtual void iEndContext(NavisworksIntegratedAPI22.InwOpState p_state)
      {
      }
      public virtual NavisworksIntegratedAPI22.nwEExportStatus iExport(NavisworksIntegratedAPI22.InwOpState p_state, int reason, string filename, NavisworksIntegratedAPI22.InwOpProgress Progress)
      {
         // This method is called when the menu item "ADSK Example - Launch Notepad" is selected
         // It will do the following
         // 1. Launch Notepad
         // 2. Write out the list of objects in each selected object path
         // 3. For each object send the list of associated model properties and attributes

         // Open Notepad
         m_process = new Process();
         m_process.StartInfo.FileName = @"Notepad.exe";
         m_process.EnableRaisingEvents = true;

         m_process.Start();
         m_process.WaitForInputIdle(1000);
         if (m_process.Responding)
         {
            // Get the set of currently selected node paths
            NavisworksIntegratedAPI22.InwOpState10 l_state = p_state as NavisworksIntegratedAPI22.InwOpState10;

            NavisworksIntegratedAPI22.InwSelectionPathsColl selPaths = p_state.CurrentSelection.Paths();
            foreach (NavisworksIntegratedAPI22.InwOaPath pPath in selPaths)
            {
               // Walk along the path writing the object information [UserName] and [ObjectName]
               NavisworksIntegratedAPI22.InwPathNodesColl pNodes = pPath.Nodes();
               foreach (NavisworksIntegratedAPI22.InwOaNode node in pNodes)
               {
                  if (node.UserName != "")
                  {
                     SendKeys.Send(String.Format("[Object] = {0}  : {1}\n", node.UserName, node.ObjectName));
                  }
                  // write out the model attribtues for the current nodes
                  SendKeys.Send("Model Attributes\n");
                  foreach (NavisworksIntegratedAPI22.InwOaAttribute nodeAttr in node.Attributes())
                  {
                     NavisworksIntegratedAPI22.InwOaPropertyVec pVec = l_state.GetAttributeProperties(nodeAttr);
                     foreach (NavisworksIntegratedAPI22.InwOaProperty nodeProp in pVec.Properties())
                     {
                        String string_val = nodeProp.value as String;
                        SendKeys.Send(String.Format("\t\t PROPERTY [name] = {0} \t [username] = {1} \t [value] = {2}\n", nodeProp.name, nodeProp.UserName, string_val));
                     }
                  }
               }
               // Show the model overrides.
               SendKeys.Send("GUI Attribute List\n");
               NavisworksIntegratedAPI22.InwGUIPropertyNode propNode = l_state.GetGUIPropertyNode(pPath, true);
               foreach (NavisworksIntegratedAPI22.InwGUIAttribute2 attr in propNode.GUIAttributes())
               {
                  SendKeys.Send(String.Format("\t [UserDefined] = {0} \t [ClassName] = {1} \t [ClassUserName] = {2} \t [Name] = {3} \n", attr.UserDefined, attr.ClassName, attr.ClassUserName, attr.name));
                  NavisworksIntegratedAPI22.InwOaPropertyColl propCol = attr.Properties();
                  foreach (NavisworksIntegratedAPI22.InwOaProperty propAttr in propCol)
                  {
                     String string_val = propAttr.value as String;
                     SendKeys.Send(String.Format("\t\t PROPERTY [name] = {0} \t [username] = {1} \t [value] = {2}\n", propAttr.name, propAttr.UserName, string_val));
                  }
               }
            }

            return NavisworksIntegratedAPI22.nwEExportStatus.eExport_OK;
         }
         else
         {
            // myProcess.Kill();
            return NavisworksIntegratedAPI22.nwEExportStatus.eExport_FAIL;
         }
         
      }
      public virtual string iGetFormatName()
      {
         return "Com exportplugin1 FN";
      }
      public virtual string iGetFormatSuffix()
      {
         return "Com exportplugin1 FS";
      }
      public virtual string iGetUserString()
      {
         // This is the identifying string that will appear
         // in the context menu.
         return "ADSK Example - Launch Notepad";
      }
      public virtual bool iHelpExport()
      {
         return false;
      }
      public virtual void InitialisePlugin(ref int capbits, ref int ver)
      {
         // Identify the plugin as a plugin context menu item.
         NavisworksIntegratedAPI22.nwECaps cb = NavisworksIntegratedAPI22.nwECaps.eCap_EXPORT_EXPLICIT | NavisworksIntegratedAPI22.nwECaps.eCap_EXPORT_CONTEXT_ITEM;
         capbits = (int)cb;
      }
   }
}
