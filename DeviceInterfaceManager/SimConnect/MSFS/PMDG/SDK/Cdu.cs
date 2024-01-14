﻿using System;
using System.Runtime.InteropServices;

namespace DeviceInterfaceManager.SimConnect.MSFS.PMDG.SDK;

public static class Cdu
{
    // CDU Screen Cell Structure
    //

    // CDU Screen Data Structure
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Screen
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public Row[] Columns;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct Row
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public Cell[] Rows;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
            public struct Cell
            {
                [MarshalAs(UnmanagedType.U1)]
                public char Symbol;

                public Color Color; // any of CDU_COLOR_ defines
                public Flag Flags;  // a combination of CDU_FLAG_ bits
            }
        }

        [MarshalAs(UnmanagedType.I1)]
        public bool Powered; // true if CDU is powered
    }

    public interface ICduScreen
    {
        public Screen Screen { get; }
    }

    // CDU Screen Cell Colors
    public enum Color : byte
    {
        White,
        Cyan,
        Green,
        Magenta,
        Amber,
        Red
    }

    // CDU Screen Cell flags
    [Flags]
    public enum Flag : byte
    {
        SmallFont = 0x01, // small font, including that used for line headers 
        Reverse = 0x02,   // character background is highlighted in reverse video
        Unused = 0x04     // dimmed character color indicating inop/unused entries
    }
}