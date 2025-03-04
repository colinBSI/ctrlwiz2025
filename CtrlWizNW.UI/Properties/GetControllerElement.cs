using CtrlWiz.NW.UI.Models;
using System;
using System.ComponentModel;
using System.Configuration;

namespace CtrlWiz.NW.UI.Properties
{
    public static class GetControllerElement
    {
        static GetControllerElement()
        {
            Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        public static ControllerButton GetControllerButtonByFunction(Function function)
        {
            ControllerButton button = ControllerButton.Unset;

            foreach (SettingsProperty prop in Settings.Default.Properties)
            {
                if (Settings.Default[prop.Name] is Function && (Function)Settings.Default[prop.Name] == function)
                {
                    button = GetControllerElementByPropertyName(prop.Name) is ControllerButton ?
                        (ControllerButton)GetControllerElementByPropertyName(prop.Name) : ControllerButton.Unset;

                    if (button != ControllerButton.Unset)
                        break;
                }
                else
                {
                    continue;
                }
            }

            return button;
        }
        public static ControllerTrigger GetControllerTriggerByFunction(Function function)
        {
            ControllerTrigger trigger = ControllerTrigger.Unset;

            foreach (SettingsProperty prop in Settings.Default.Properties)
            {
                if (Settings.Default[prop.Name] is Function && (Function)Settings.Default[prop.Name] == function)
                {
                    trigger = GetControllerElementByPropertyName(prop.Name) is ControllerTrigger ?
                        (ControllerTrigger)GetControllerElementByPropertyName(prop.Name) : ControllerTrigger.Unset;

                    if (trigger != ControllerTrigger.Unset)
                        break;
                }
                else
                {
                    continue;
                }
            }

            return trigger;
        }
        public static ControllerThumbStick GetControllerThumbStickByFunction(Function function)
        {
            ControllerThumbStick thumbStick = ControllerThumbStick.Unset;

            foreach (SettingsProperty prop in Settings.Default.Properties)
            {
                if (Settings.Default[prop.Name] is Function && (Function)Settings.Default[prop.Name] == function)
                {
                    thumbStick = GetControllerElementByPropertyName(prop.Name) is ControllerThumbStick ?
                        (ControllerThumbStick)GetControllerElementByPropertyName(prop.Name) : ControllerThumbStick.Unset;

                    if (thumbStick != ControllerThumbStick.Unset)
                        break;
                }
                else
                {
                    continue;
                }
            }

            return thumbStick;
        }

        public static object GetControllerElementByPropertyName(string propertyName)
        {
            if (propertyName == "NV_AButton")
                return ControllerButton.A;
            else if (propertyName == "NV_BButton")
                return ControllerButton.B;
            else if (propertyName == "NV_XButton")
                return ControllerButton.X;
            else if (propertyName == "NV_YButton")
                return ControllerButton.Y;
            else if (propertyName == "NV_RBumber")
                return ControllerButton.RBumber;
            else if (propertyName == "NV_StartButton")
                return ControllerButton.Start;
            else if (propertyName == "NV_LTrigger")
                return ControllerTrigger.LT;
            else if (propertyName == "NV_RTrigger")
                return ControllerTrigger.RT;
            else if (propertyName == "NV_LStick")
                return ControllerThumbStick.L;
            else if (propertyName == "NV_RStick")
                return ControllerThumbStick.R;
            else
                return null;
        }
        public static string GetPropertyNameByControllerElement(Enum element)
        {
            if (element is ControllerButton)
            {
                if ((ControllerButton)element == ControllerButton.A)
                    return "NV_AButton";
                else if ((ControllerButton)element == ControllerButton.B)
                    return "NV_BButton";
                else if ((ControllerButton)element == ControllerButton.X)
                    return "NV_XButton";
                else if ((ControllerButton)element == ControllerButton.Y)
                    return "NV_YButton";
                else if ((ControllerButton)element == ControllerButton.RBumber)
                    return "NV_RBumber";
                else if ((ControllerButton)element == ControllerButton.Start)
                    return "NV_StartButton";
                else
                    return null;
            }
            else if (element is ControllerTrigger)
            {
              if ((ControllerTrigger)element == ControllerTrigger.LT)
                    return "NV_LTrigger";
                else if ((ControllerTrigger)element == ControllerTrigger.RT)
                    return "NV_RTrigger";
                else
                    return null;
            }
            else
            {
                if ((ControllerThumbStick)element == ControllerThumbStick.L)
                    return "NV_LStick";
                else if ((ControllerThumbStick)element == ControllerThumbStick.R)
                    return "NV_RStick";
                else
                    return null;
            }
        }

        private static void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SettingsPropertyChanged != null)
                SettingsPropertyChanged(null, e);
        }

        public static event PropertyChangedEventHandler SettingsPropertyChanged;
    }
}

#region Unused Code
//public static Function GetControllerElementFunctionByPropertyNameNV(string propertyName)
//{
//    if (propertyName == "NV_AButton")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_BButton")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_XButton")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_YButton")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_RBumber")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_StartButton")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_LTrigger")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_RTrigger")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_LStick")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else if (propertyName == "NV_RStick")
//        return Settings.Default[propertyName] is Function ? (Function)Settings.Default[propertyName] : Function.Unset;
//    else
//        return Function.Unset;
//}
#endregion