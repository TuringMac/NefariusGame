package ru.fluxbit.nefariusmobapp;

public class Player {
    public String ID;
    public int Color;
    public String Name = "";
    public int Coins = 0;
    public int[] Spies;
    public int InventionCount;
    public int Score;
    public boolean IsMoved;
    public Invention[] PlayedInventions;
    public EffectDescription[] EffectQueue;
    public Effect CurrentEffect;
    public boolean HasEffect;
}
