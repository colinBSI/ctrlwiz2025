using System;

namespace CtrlWiz.NW.UI.Models
{
    [Serializable]
    public enum Function
    {
        EnableTextMarkup = 0,
        CreateViewpoint = 1,
        ShowHideProperties = 2,
        ShowHideSelectionTree = 3,
        ShowHideViewpoints = 4,
        MoveLeftInTree = 5,
        PersonView3rd = 6,
        CollisionGravity = 7,
        HideUnhide = 8,
        DeselectAll = 9,
        SelectDeselect = 10,
        UnhideAll = 11,
        MoveSprint = 12,
        Look = 13,
        LookInvertedY = 14,
        ElevationUp = 15,
        ElevationDown = 16,
        NextViewpoint = 17,
        ShortestDistance = 18,
        LeftDroneControls = 19,
        RigthDroneControls = 20,
        None = 21,
        Unset = -1
    }
}