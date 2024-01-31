using System.Diagnostics.CodeAnalysis;

namespace DeviceInterfaceManager.Models.Devices.interfaceIT.USB;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum InterfaceItBoardId : ushort
{
    //Board type identifiers
    INTERFACEIT_BOARD_ALL = 0,

    // Misc
    // FDS-MFP - Original version
    FDS_MFP = 0x0201,

    // FDS-SYS boards - Older original boards
    FDSSYS1 = 0x32E1,

    FDSSYS2 = 0x32E2,

    FDSSYS3 = 0x32E0,

    FDSSYS4 = 0x32E3,

    //Radios
    // Base
    RADIO_CODE = 0x4C00,

    // 737 NAV v1
    FDS_737NG_NAV = 0x4C55,
    FDS_737NG_NAV_ID = 0x0055,

    // 737 COMM v1
    FDS_737NG_COMM = 0x4C56,
    FDS_737NG_COMM_ID = 0x0056,

    // A320 Multi v1
    FDS_A320_MULTI = 0x4C57,
    FDS_A320_MULTI_ID = 0x0057,

    // 737 NAV v2
    FDS_737NG_NAV_V2 = 0x4C58,
    FDS_737NG_NAV_V2_ID = 0x0058,

    // 737 COMM v2
    FDS_737NG_COMM_V2 = 0x4C59,
    FDS_737NG_COMM_V2_ID = 0x0059,

    // 737 NAV / COMM
    FDS_737NG_NAVCOMM = 0x4C5A,
    FDS_737NG_NAVCOMM_ID = 0x005A,

    // FDS-CDU
    FDS_CDU = 0x3302,

    // FDS-A-ACP
    FDS_A_ACP = 0x3303,

    // FDS-A-RMP
    FDS_A_RMP = 0x3304,

    // FDS-XPNDR
    FDS_XPNDR = 0x3305,

    // FDS-737NG-ELECT
    FDS_737_ELECT = 0x3306,

    // FS-MFP v2
    MFP_V2 = 0x3307,

    // FDS-CONTROLLER-MCP
    FDS_CONTROLLER_MCP = 0x330A,

    // FDS-CONTROLLER-EFIS-CA
    FDS_CONTROLLER_EFIS_CA = 0x330B,

    // FDS-CONTROLLER-EFIS-FO
    FDS_CONTROLLER_EFIS_FO = 0x330C,

    // FDS-737NG-MCP-ELVL - Not production
    FDS_737NG_MCP_ELVL = 0x3310,

    // FDS-737-EL-MCP / FDS-737-MX-MCP
    FDS_737_MX_MCP = 0x3311,

    // FDS-787-MCP
    FDS_787NG_MCP = 0x3319,

    // FDS-A320-FCU
    FDS_A320_FCU = 0x3316,

    // FDS_747_RADIO (MULTI_COMM)
    FDS_7X7_MCOMM = 0x3318,

    // FDS-CDU v9

    FDS_CDU_V9 = 0x331A,

    // FDS-A-TCAS V1

    FDS_A_TCAS = 0x331B,

    // FDS-A-ECAM v1

    FDS_A_ECAM = 0x331C,

    // FDS-A-CLOCK v1
    FDS_A_CLOCK = 0x331D,

    FDS_A_RMP_V2 = 0x331E,

    FDS_A_ACP_V2 = 0x331F,

    A320_PEDESTAL = 0x3320,

    FDS_A320_35VU = 0x3321,

    FDS_IRS = 0x3322,

    FDS_OM1 = 0x3323,

    FDS_OE1 = 0x3324,

    FDS_GM1 = 0x3325,

    FDS_NDF_GDS_NCP = 0x3326,

    FDS_777_MX_MCP = 0x3327,

    FDS_DCP_EFIS = 0x3328,

    FDS_747_MX_MCP = 0x3329,

    // 5 Position EFIS
    FDS_737_PMX_EFIS_5_CA = 0x332A,

    // 5 Position EFIS
    FDS_737_PMX_EFIS_5_FO = 0x332B,

    // 737 Pro MX MCP
    FDS_737_PMX_MCP = 0x332C,

    // 737 Pro MX EFIS (Encoder) - CA
    FDS_737_PMX_EFIS_E_CA = 0x332D,

    // 737 Pro MX EFIS (Encoder) - FO
    FDS_737_PMX_EFIS_E_FO = 0x332E,

    // 737 MAX 
    FDS_737_MAX_ABRAKE_EFIS = 0x332F,

    // 787 Tuning and Control Panel
    FDS_787_TCP = 0x3330,

    // C17 AFCSP
    FDS_C17_AFCSP = 0x33EF,

    // JetMAX Boards
    JetMAX_737_MCP = 0x330F,

    JetMAX_737_RADIO = 0x3401,

    JetMAX_737_XPNDR = 0x3402,

    JetMAX_777_MCP = 0x3403,

    JetMAX_737_MCP_V2 = 0x3404,

    // interfaceIT™ Boards
    IIT_HIO_32_64 = 0x4101,

    IIT_HIO_64_128 = 0x4102,

    IIT_HIO_128_256 = 0x4103,

    IIT_HI_128 = 0x4105,

    IIT_HRI_8 = 0x4106,

    IIT_RELAY_8 = 0x4107,

    IIT_DEV = 0x4108,

    HIO_RELAY_8 = 0x4109,
}