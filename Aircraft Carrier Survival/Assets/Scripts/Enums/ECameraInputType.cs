using System;

[Flags]
public enum ECameraInputType
{
    Keyboard        = 1 << 0,
    Mouse           = 1 << 1,
    InputSwitch     = 1 << 2,
    ClickSwitch     = 1 << 3,
    ScrollSwitch    = 1 << 4,
    FreeView        = 1 << 28,
    IslandView      = 1 << 29,
    SectionsView    = 1 << 30,
    DeckView        = 1 << 31,
}
