//------------------------------------------------------------------------------
//
//  PMDG 777 external connection SDK
//  Copyright (c) 2024 Precision Manuals Development Group
//
//  Converted from unmanaged to managed code by Oudoum
// 
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

public static class B777
{
    public const string DataName = "PMDG_777X_Data";
    public const string ControlName = "PMDG_777X_Control";
    public const string Cdu0Name = "PMDG_777X_CDU_0";
    public const string Cdu1Name = "PMDG_777X_CDU_1";
    public const string Cdu2Name = "PMDG_777X_CDU_2";
    
    public enum ClientDataId
    {
        Data = 0x504D4447,
        Control = 0x504D4449,
        Cdu0 = 0x4E477835,
        Cdu1 = 0x4E477836,
        Cdu2 = 0x4E477837
    }
    
    public enum DefineId
    {
        Data = 0x504D4448,
        Control = 0x504D444A,
        Cdu0 = 0x4E477838,
        Cdu1 = 0x4E477839,
        Cdu2 = 0x4E47783A
    }
    
    // NOTE - add these lines to the 777_Options.ini file: 
    //
    //[SDK]
    //EnableDataBroadcast=1
    //
    // to enable the aircraft data sending from the 777.
    //
    //
    // Add any of these lines to the [SDK] section of the 777_Options.ini file: 
    //
    //EnableCDUBroadcast.0=1 
    //EnableCDUBroadcast.1=1 
    //EnableCDUBroadcast.2=1 
    //
    // to enable the contents of the corresponding CDU screen to be sent to external programs.


    // 777 data structure
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Data
    {
        ////////////////////////////////////////////
        // Controls and indicators
        ////////////////////////////////////////////

        // Overhead Maintenance Panel
        //------------------------------------------

        // Backup Window Heat
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ICE_WindowHeatBackUp_Sw_OFF;

        // Standby Power
        public byte ELEC_StandbyPowerSw;              // 0: OFF  1: AUTO  2: BAT

        // Flight Controls			
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] FCTL_WingHydValve_Sw_SHUT_OFF; // left/right/center	
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] FCTL_TailHydValve_Sw_SHUT_OFF; // left/right/center	
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] FCTL_annunTailHydVALVE_CLOSED; // left/right/center	
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] FCTL_annunWingHydVALVE_CLOSED; // left/right/center	
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_PrimFltComputersSw_AUTO;                                                            // true: AUTO  false: DISC
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_annunPrimFltComputersDISC;
        
        // APU MAINT
        [MarshalAs(UnmanagedType.I1)] public bool APU_Power_Sw_TEST;

        // EEC MAINT
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ENG_EECPower_Sw_TEST;

        // ELECTRICAL
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_TowingPower_Sw_BATT;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunTowingPowerON_BATT;

        // CARGO TEMP SELECTORS
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] AIR_CargoTemp_Selector; // aft / bulk  0=OFF  1=LOW  2=HIGH   AFT/BULK
        public byte AIR_CargoTemp_MainDeckFwd_Sel;                                                 // 0: C ... 60: W
        public byte AIR_CargoTemp_MainDeckAft_Sel;                                                 // 0: C ... 60: W
        public byte AIR_CargoTemp_LowerFwd_Sel;                                                    // 0: C ... 60: W
        public byte AIR_CargoTemp_LowerAft_Sel;                                                    // 0: C ... 60: W  67: HEAT H  70: HEAT OFF  73: HEAT L

        // Overhead Panel
        //------------------------------------------

        // ADIRU
        [MarshalAs(UnmanagedType.I1)] public bool ADIRU_Sw_On;
        [MarshalAs(UnmanagedType.I1)] public bool ADIRU_annunOFF;
        [MarshalAs(UnmanagedType.I1)] public bool ADIRU_annunON_BAT;

        // Flight Controls
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_ThrustAsymComp_Sw_AUTO;
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_annunThrustAsymCompOFF;

        // Electrical
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_CabUtilSw;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunCabUtilOFF;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_IFEPassSeatsSw;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunIFEPassSeatsOFF;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_Battery_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunBattery_OFF;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunAPU_GEN_OFF;
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_APUGen_Sw_ON;
        public byte ELEC_APU_Selector;                    // 0: OFF  1: ON  2: START	
        [MarshalAs(UnmanagedType.I1)] public bool ELEC_annunAPU_FAULT;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_BusTie_Sw_AUTO;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunBusTieISLN;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_ExtPwrSw;                  // primary/secondary - MOMENTARY SWITCHES
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunExtPowr_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunExtPowr_AVAIL;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_Gen_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunGenOFF;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_BackupGen_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunBackupGenOFF;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_IDGDiscSw;                 // MOMENTARY SWITCHES
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ELEC_annunIDGDiscDRIVE;

        // Wiper Selectors
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] WIPERS_Selector;                   // left/right   0: OFF  1: INT  2: LOW  3:HIGH

        // Emergency Lights
        public byte LTS_EmerLightsSelector;               // 0: OFF  1: ARMED  2: ON

        // Service Interphone
        [MarshalAs(UnmanagedType.I1)] public bool COMM_ServiceInterphoneSw;

        // Passenger Oxygen
        [MarshalAs(UnmanagedType.I1)] public bool OXY_PassOxygen_Sw_On;
        [MarshalAs(UnmanagedType.I1)] public bool OXY_annunPassOxygenON;

        // Window Heat
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 4)] public bool[] ICE_WindowHeat_Sw_ON;           // L-Side/L-Fwd/R-Fwd/R-Side
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 4)] public bool[] ICE_annunWindowHeatINOP;            // L-Side/L-Fwd/R-Fwd/R-Side

        // Hydraulics
        [MarshalAs(UnmanagedType.I1)] public bool HYD_RamAirTurbineSw;
        [MarshalAs(UnmanagedType.I1)] public bool HYD_annunRamAirTurbinePRESS;
        [MarshalAs(UnmanagedType.I1)] public bool HYD_annunRamAirTurbineUNLKD;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_PrimaryEngPump_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_PrimaryElecPump_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] HYD_DemandElecPump_Selector;       // 0: OFF  1: AUTO  2: ON
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] HYD_DemandAirPump_Selector;        // 0: OFF  1: AUTO  2: ON
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_annunPrimaryEngPumpFAULT;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_annunPrimaryElecPumpFAULT;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_annunDemandElecPumpFAULT;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] HYD_annunDemandAirPumpFAULT;

        // Passenger Signs
        public byte SIGNS_NoSmokingSelector;          // 0: OFF  1: AUTO   2: ON
        public byte SIGNS_SeatBeltsSelector;          // 0: OFF  1: AUTO   2: ON

        // Flight Deck Lights
        public byte LTS_DomeLightKnob;                    // Position 0...100
        public byte LTS_CircuitBreakerKnob;               // Position 0...100
        public byte LTS_OvereadPanelKnob;             // Position 0...100
        public byte LTS_GlareshieldPNLlKnob;          // Position 0...100
        public byte LTS_GlareshieldFLOODKnob;         // Position 0...100
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Storm_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_MasterBright_Sw_ON;
        public byte LTS_MasterBrigntKnob;             // Position 0...100
        public byte LTS_IndLightsTestSw;              // 0: TEST  1: BRT  2: DIM

        // Exterior Lights
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] LTS_LandingLights_Sw_ON;            // Left/Right/Nose 
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Beacon_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_NAV_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Logo_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Wing_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] LTS_RunwayTurnoff_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Taxi_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool LTS_Strobe_Sw_ON;

        // Engine, APU and Cargo Fire
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_CargoFire_Sw_Arm;          // FWD/AFT
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_annunCargoFire;                // FWD/AFT
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_CargoFireDisch_Sw;                // MOMENTARY SWITCH
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_annunCargoDISCH;
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_FireOvhtTest_Sw;              // MOMENTARY SWITCH
        public byte FIRE_APUHandle;                       // 0: IN (NORMAL)  1: PULLED OUT  2: TURNED LEFT  3: TURNED RIGHT  (2 & 3 ane momnentary positions)
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_APUHandleUnlock_Sw;           // MOMENTARY SWITCH resets when handle pulled
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_annunAPU_BTL_DISCH;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_EngineHandleIlluminated;
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_APUHandleIlluminated;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_EngineHandleIsUnlocked;
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_APUHandleIsUnlocked;
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_annunMainDeckCargoFire;
        [MarshalAs(UnmanagedType.I1)] public bool FIRE_annunCargoDEPR;               // DEPR light in DEPR/DISCH guarded switch

        // Engine
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ENG_EECMode_Sw_NORM;                // left / right
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] ENG_Start_Selector;                // left / right  0: START  1: NORM
        [MarshalAs(UnmanagedType.I1)] public bool ENG_Autostart_Sw_ON;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ENG_annunALTN;
        [MarshalAs(UnmanagedType.I1)] public bool ENG_annunAutostartOFF;

        // Fuel
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_CrossFeedFwd_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_CrossFeedAft_Sw;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_PumpFwd_Sw;                    // left fwd / right fwd
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_PumpAft_Sw;                    // left aft / right aft
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_PumpCtr_Sw;                    // ctr left / ctr right
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_JettisonNozle_Sw;          // left / right
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_JettisonArm_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_FuelToRemain_Sw_Pulled;
        public byte FUEL_FuelToRemain_Selector;           // 0: DECR  1: Neutral  2: INCR
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_annunFwdXFEED_VALVE;
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_annunAftXFEED_VALVE;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_annunLOWPRESS_Fwd;         // left fwd / right fwd
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_annunLOWPRESS_Aft;         // left aft / right aft
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_annunLOWPRESS_Ctr;         // ctr left / ctr right
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FUEL_annunJettisonNozleVALVE;   // left / right
        [MarshalAs(UnmanagedType.I1)] public bool FUEL_annunArmFAULT;

        // Anti-Ice
        public byte ICE_WingAntiIceSw;                    // 0: OFF  1: AUTO  2: ON
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] ICE_EngAntiIceSw;              // 0: OFF  1: AUTO  2: ON


        // Air Conditioning
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_Pack_Sw_AUTO;               // left / right
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_TrimAir_Sw_On;              // left / right
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_RecircFan_Sw_On;                // upper / lower	
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] AIR_TempSelector;              // flt deck / cabin  0: C ... 60: W ... 70: MAN (flt deck only)  	
        [MarshalAs(UnmanagedType.I1)] public bool AIR_AirCondReset_Sw_Pushed;            // MOMENTARY action		
        [MarshalAs(UnmanagedType.I1)] public bool AIR_EquipCooling_Sw_AUTO;
        [MarshalAs(UnmanagedType.I1)] public bool AIR_Gasper_Sw_On;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_annunPackOFF;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_annunTrimAirFAULT;
        [MarshalAs(UnmanagedType.I1)] public bool AIR_annunEquipCoolingOVRD;
        [MarshalAs(UnmanagedType.I1)] public bool AIR_AltnVentSw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool AIR_annunAltnVentFAULT;
        [MarshalAs(UnmanagedType.I1)] public bool AIR_MainDeckFlowSw_NORM;           // M/D FLOW  true: NORM  false: HIGH

        // Bleed Air
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_EngBleedAir_Sw_AUTO;            // left engine / right engine
        [MarshalAs(UnmanagedType.I1)] public bool AIR_APUBleedAir_Sw_AUTO;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_IsolationValve_Sw;          // left / right 
        [MarshalAs(UnmanagedType.I1)] public bool AIR_CtrIsolationValve_Sw;          // left / right 
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_annunEngBleedAirOFF;               // left engine / right engine
        [MarshalAs(UnmanagedType.I1)] public bool AIR_annunAPUBleedAirOFF;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_annunIsolationValveCLOSED;  // left / right 
        [MarshalAs(UnmanagedType.I1)] public bool AIR_annunCtrIsolationValveCLOSED;


        // Pressurization
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_OutflowValve_Sw_AUTO;       // fwd / aft
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] AIR_OutflowValveManual_Selector;   // fwd / aft   0: OPEN  1: Neutral  2: CLOSE
        [MarshalAs(UnmanagedType.I1)] public bool AIR_LdgAlt_Sw_Pulled;
        public byte AIR_LdgAlt_Selector;              // 0: DECR  1: Neutral  2: INCR
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] AIR_annunOutflowValve_MAN;      // fwd / aft



        // Forward panel
        //------------------------------------------

        // Center 
        public byte GEAR_Lever;                           // 0: UP  1: DOWN
        [MarshalAs(UnmanagedType.I1)] public bool GEAR_LockOvrd_Sw;                  // MOMENTARY SWITCH (resets when gear lever moved)
        [MarshalAs(UnmanagedType.I1)] public bool GEAR_AltnGear_Sw_DOWN;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_FlapInhibitSw_OVRD;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_GearInhibitSw_OVRD;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_TerrInhibitSw_OVRD;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_RunwayOvrdSw_OVRD;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_GSInhibit_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_annunGND_PROX_top;
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_annunGND_PROX_bottom;
        public byte BRAKES_AutobrakeSelector;         // 0: RTO  1: OFF  2: DISARM   3: "1" ... 5: MAX AUTO

        // Standby - ISFD  (all are MOMENTARY action switches)
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_Baro_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_RST_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_Minus_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_Plus_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_APP_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool ISFD_HP_IN_Sw_Pushed;

        // Left 
        [MarshalAs(UnmanagedType.I1)] public bool ISP_Nav_L_Sw_CDU;
        [MarshalAs(UnmanagedType.I1)] public bool ISP_DsplCtrl_L_Sw_Altn;
        [MarshalAs(UnmanagedType.I1)] public bool ISP_AirDataAtt_L_Sw_Altn;
        public byte DSP_InbdDspl_L_Selector;          //0: ND  1: NAV  2: MFD  3: EICAS
        [MarshalAs(UnmanagedType.I1)] public bool EFIS_HdgRef_Sw_Norm;
        [MarshalAs(UnmanagedType.I1)] public bool EFIS_annunHdgRefTRUE;
        public int BRAKES_BrakePressNeedle;            // Value 0...100 (corresponds to 0...4000 PSI)
        [MarshalAs(UnmanagedType.I1)] public bool BRAKES_annunBRAKE_SOURCE;

        // Right 
        [MarshalAs(UnmanagedType.I1)] public bool ISP_Nav_R_Sw_CDU;
        [MarshalAs(UnmanagedType.I1)] public bool ISP_DsplCtrl_R_Sw_Altn;
        [MarshalAs(UnmanagedType.I1)] public bool ISP_AirDataAtt_R_Sw_Altn;
        public byte ISP_FMC_Selector;                 //0: LEFT   1: AUTO  2: RIGHT
        public byte DSP_InbdDspl_R_Selector;          //0: EICAS  1: MFD   2: ND  3: PFD

        // Left & Right Sidewalls
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] AIR_ShoulderHeaterKnob;            // Left / Right  Position 0...100
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] AIR_FootHeaterSelector;            // Left / Right  0: OFF  1: LOW  2: HIGH
        public byte LTS_LeftFwdPanelPNLKnob;          // Position 0...100
        public byte LTS_LeftFwdPanelFLOODKnob;            // Position 0...100
        public byte LTS_LeftOutbdDsplBRIGHTNESSKnob;  // Position 0...100
        public byte LTS_LeftInbdDsplBRIGHTNESSKnob;       // Position 0...100

        public byte LTS_RightFwdPanelPNLKnob;         // Position 0...100
        public byte LTS_RightFwdPanelFLOODKnob;           // Position 0...100	
        public byte LTS_RightInbdDsplBRIGHTNESSKnob;  // Position 0...100
        public byte LTS_RightOutbdDsplBRIGHTNESSKnob; // Position 0...100


        // Chronometers (Left / Right)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] CHR_Chr_Sw_Pushed;              // MOMENTARY SWITCH
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] CHR_TimeDate_Sw_Pushed;         // MOMENTARY SWITCH
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] CHR_TimeDate_Selector;         // 0: UTC  1: MAN
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] CHR_Set_Selector;              // 0: RUN  1: HLDY  2: MM  3: HD
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] CHR_ET_Selector;                   // 0: RESET (MOMENTARY spring-loaded to HLD)  1: HLD  2: RUN


        // Glareshield
        //------------------------------------------

        // EFIS switches (left / right)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_MinsSelBARO;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_BaroSelHPA;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_VORADFSel1;                   // 0: VOR  1: OFF  2: ADF
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_VORADFSel2;                   // 0: VOR  1: OFF  2: ADF
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_ModeSel;                  // 0: APP  1: VOR  2: MAP  3: PLAN
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_RangeSel;                 // 0: 10 ... 6: 640
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_MinsKnob;                 // 0..99
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] EFIS_BaroKnob;                 // 0..99

        // EFIS MOMENTARY action (left / right)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_MinsRST_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_BaroSTD_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_ModeCTR_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_RangeTFC_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_WXR_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_STA_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_WPT_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_ARPT_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_DATA_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_POS_Sw_Pushed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_TERR_Sw_Pushed;

        // MCP - Variables
        public float MCP_IASMach;                      // Mach if < 10.0
        [MarshalAs(UnmanagedType.I1)] public bool MCP_IASBlank;
        public ushort MCP_Heading;
        public ushort MCP_Altitude;
        public short MCP_VertSpeed;
        public float MCP_FPA;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_VertSpeedBlank;

        // MCP - Switches
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] MCP_FD_Sw_On;                   // left / right
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] MCP_ATArm_Sw_On;                    // left / right
        public byte MCP_BankLimitSel;                 // 0: AUTO  1: 5  2: 10 ... 5: 25
        [MarshalAs(UnmanagedType.I1)] public bool MCP_AltIncrSel;                        // false: AUTO  true: 1000
        [MarshalAs(UnmanagedType.I1)] public bool MCP_DisengageBar;
        public byte MCP_Speed_Dial;                       // 0 ... 99
        public byte MCP_Heading_Dial;                 // 0 ... 99
        public byte MCP_Altitude_Dial;                    // 0 ... 99
        public byte MCP_VS_Wheel;                     // 0 ... 99

        public byte MCP_HDGDial_Mode;                 // 0: Dial shows HDG, 1: Dial shows TRK
        public byte MCP_VSDial_Mode;                  // 0: Dial shows VS, 1: Dial shows FPA

        // MCP - MOMENTARY action switches
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] MCP_AP_Sw_Pushed;               // left / right
        [MarshalAs(UnmanagedType.I1)] public bool MCP_CLB_CON_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_AT_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_LNAV_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_VNAV_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_FLCH_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_HDG_HOLD_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_VS_FPA_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_ALT_HOLD_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_LOC_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_APP_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_Speeed_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_Heading_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_Altitude_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_IAS_MACH_Toggle_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_HDG_TRK_Toggle_Sw_Pushed;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_VS_FPA_Toggle_Sw_Pushed;

        // MCP - Annunciator lights
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] MCP_annunAP;                        // left / right
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunAT;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunLNAV;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunVNAV;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunFLCH;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunHDG_HOLD;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunVS_FPA;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunALT_HOLD;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunLOC;
        [MarshalAs(UnmanagedType.I1)] public bool MCP_annunAPP;

        // Display Select Panel	- These are all MOMENTARY SWITCHES
        [MarshalAs(UnmanagedType.I1)] public bool DSP_L_INBD_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_R_INBD_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_LWR_CTR_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_ENG_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_STAT_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_ELEC_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_HYD_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_FUEL_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_AIR_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_DOOR_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_GEAR_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_FCTL_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_CAM_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_CHKL_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_COMM_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_NAV_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_CANC_RCL_Sw;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_annunL_INBD;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_annunR_INBD;
        [MarshalAs(UnmanagedType.I1)] public bool DSP_annunLWR_CTR;

        // Master Warning/Caution
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] WARN_Reset_Sw_Pushed;           // MOMENTARY action
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] WARN_annunMASTER_WARNING;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] WARN_annunMASTER_CAUTION;


        // Forward Aisle Stand Panel
        //------------------------------------------

        [MarshalAs(UnmanagedType.I1)] public bool ISP_DsplCtrl_C_Sw_Altn;
        public byte LTS_UpperDsplBRIGHTNESSKnob;      // Position 0...100
        public byte LTS_LowerDsplBRIGHTNESSKnob;      // Position 0...100
        [MarshalAs(UnmanagedType.I1)] public bool EICAS_EventRcd_Sw_Pushed;          // MOMENTARY action		

        // CDU (Left/Right/Center)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] CDU_annunEXEC;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] CDU_annunDSPY;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] CDU_annunFAIL;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] CDU_annunMSG;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] CDU_annunOFST;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] CDU_BrtKnob;                       // 0: DecreasePosition 1: Neutral  2: Increase


        // Control Stand
        //------------------------------------------

        [MarshalAs(UnmanagedType.I1)] public bool FCTL_AltnFlaps_Sw_ARM;
        public byte FCTL_AltnFlaps_Control_Sw;            // 0: RET  1: OFF  2: EXT
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_StabCutOutSw_C_NORMAL;
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_StabCutOutSw_R_NORMAL;
        public byte FCTL_AltnPitch_Lever;             // 0: NOSE DOWN  1: NEUTRAL  2: NOSE UP
        public byte FCTL_Speedbrake_Lever;                // Position 0...100  0: DOWN,  25: ARMED, 26...100: DEPLOYED 
        public byte FCTL_Flaps_Lever;                 // 0: UP  1: 1  2: 5  3: 15  4: 20  5: 25  6: 30 	
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ENG_FuelControl_Sw_RUN;
        [MarshalAs(UnmanagedType.I1)] public bool BRAKES_ParkingBrakeLeverOn;


        // Aft Aisle Stand Panel
        //------------------------------------------

        // Audio Control Panels								// Comm Systems: 0=VHFL 1=VHFC 2=VHFR 3=FLT 4=CAB 5=PA 6=HFL 7=HFR 8=SAT1 9=SAT2 10=SPKR 11=VOR/ADF 12=APP
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] COMM_SelectedMic;              // array: 0=capt, 1=F/O, 2=observer  values: 0..9 (VHF..SAT2) as listed above; -1 if no MIC is selected
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public ushort[] COMM_ReceiverSwitches;            // array: 0=capt, 1=F/O, 2=observer.  Bit mask for selected receivers with bits indicating: 0=VHFL 1=VHFC 2=VHFR 3=FLT 4=CAB 5=PA 6=HFL 7=HFR 8=SAT1 9=SAT2 10=SPKR 11=VOR/ADF 12=APP
        public byte COMM_OBSAudio_Selector;               // 0: CAPT  1: NORMAL  2: F/O

        // Radio Control Panels								// arrays: 0=capt, 1=F/O, 2=observer
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] COMM_SelectedRadio;                // 0: VHFL  1: VHFC  2: VHFL  3: HFL  5: HFR (4 not used)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] COMM_RadioTransfer_Sw_Pushed;   // MOMENTARY action
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] COMM_RadioPanelOff;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 3)] public bool[] COMM_annunAM;

        // TCAS Panel
        [MarshalAs(UnmanagedType.I1)] public bool XPDR_XpndrSelector_R;              // true: R     false: L
        [MarshalAs(UnmanagedType.I1)] public bool XPDR_AltSourceSel_ALTN;                // true: ALTN  false: NORM  
        public byte XPDR_ModeSel;                     // 0: STBY  1: ALT RPTG OFF  2: XPNDR  3: TA ONLY  4: TA/RA
        [MarshalAs(UnmanagedType.I1)] public bool XPDR_Ident_Sw_Pushed;              // MOMENTARY action

        // Engine Fire 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] FIRE_EngineHandle;             // ENG 1/ENG2   0: IN (NORMAL)  1: PULLED OUT  2: TURNED LEFT  3: TURNED RIGHT  (2 & 3 are momenentary positions)
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_EngineHandleUnlock_Sw;     // ENG 1/ENG2   MOMENTARY SWITCH resets when handle pulled
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] FIRE_annunENG_BTL_DISCH;            // ENG 1/ENG2

        // Aileron & Rudder Trim
        public byte FCTL_AileronTrim_Switches;            // 0: LEFT WING DOWN  1: NEUTRAL  2: RIGHT WING DOWN (both switches move together)
        public byte FCTL_RudderTrim_Knob;             // 0: NOSE LEFT  1: NEUTRAL  2: NOSE RIGHT
        [MarshalAs(UnmanagedType.I1)] public bool FCTL_RudderTrimCancel_Sw_Pushed;   // MOMENTARY action

        // Evacuation Panel
        [MarshalAs(UnmanagedType.I1)] public bool EVAC_Command_Sw_ON;
        [MarshalAs(UnmanagedType.I1)] public bool EVAC_PressToTest_Sw_Pressed;
        [MarshalAs(UnmanagedType.I1)] public bool EVAC_HornSutOff_Sw_Pulled;
        [MarshalAs(UnmanagedType.I1)] public bool EVAC_LightIlluminated;


        // Aisle Stand PNL/FLOOD & Floor lights
        public byte LTS_AisleStandPNLKnob;                // Position 0...100
        public byte LTS_AisleStandFLOODKnob;          // Position 0...100
        public byte LTS_FloorLightsSw;                    // 0: BRT  1: OFF  2: DIM


        // Door state
        // Possible values are, 0: open, 1: closed, 2: closed and armed, 3: closing, 4: opening.
        // The array contains these doors:
        //  0: Entry 1L,
        //  1: Entry 1R,
        //  2: Entry 2L,
        //  3: Entry 2R,
        //  4: Entry 3L,				(This is the door aft of the wing. It is marked 4L on -300)
        //  5: Entry 3R,		
        //  6: Entry 4L,				(marked 5L on -300)
        //  7: Entry 4R,
        //  8: Entry 5L,
        //  9: Entry 5R,
        // 10: Cargo Fwd,
        // 11: Cargo Aft,
        // 12: Cargo Main,				(Freighter)
        // 13: Cargo Bulk,
        // 14: Avionics Access,
        // 15: EE Access
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] DOOR_state;
        [MarshalAs(UnmanagedType.I1)] public bool DOOR_CockpitDoorOpen;
        
        // Additional variables
        //------------------------------------------

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] ENG_StartValve;                 // true: valve open
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] AIR_DuctPress;                 // PSI
        public float FUEL_QtyCenter;                       // LBS
        public float FUEL_QtyLeft;                     // LBS
        public float FUEL_QtyRight;                        // LBS
        public float FUEL_QtyAux;                      // LBS
        [MarshalAs(UnmanagedType.I1)] public bool IRS_aligned;                       // at least one IRU is aligned
        
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_BaroMinimumsSet;			// left/right control panel. Note: check EFIS_MinsSelBARO[2] to determine if the active minimums is BARO or RADIO
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public int[] EFIS_BaroMinimums;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 2)] public bool[] EFIS_RadioMinimumsSet;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public int[] EFIS_RadioMinimums;

        // Display formats selected on the display units
        // Values are:
        // 	0:	Off,
        // 	1:	Blank,
        // 	2:	PFD,
        // 	3:	ND,
        // 	4:	EICAS,
        // 	5:	ENG,
        // 	6:	STAT,
        // 	7:	CHKL,
        // 	8:	COMM,
        // 	9:	CAM,
        // 10:	ELEC,
        // 11:	HYD,
        // 12:	FUEL,
        // 13:	AIR,
        // 14:	DOOR,
        // 15:	GEAR,
        // 16:	FCTL
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] EFIS_Display;					// Display units:  0: Capt outboard, 1: Capt inboard, 2: Upper, 3: Lower, 4: FO Inboard, 5: FO Outboard
        
        public byte AircraftModel;                        // 1: -200  2: -200ER  3: -300  4: -200LR  5: 777F  6: -300ER
        [MarshalAs(UnmanagedType.I1)] public bool WeightInKg;                            // false: LBS  true: KG
        [MarshalAs(UnmanagedType.I1)] public bool GPWS_V1CallEnabled;                    // GPWS V1 call-out option enabled
        [MarshalAs(UnmanagedType.I1)] public bool GroundConnAvailable;               // can connect/disconnect ground air/electrics

        public byte FMC_TakeoffFlaps;        // degrees, 0 if not set
        public byte FMC_V1;                  // knots, 0 if not set
        public byte FMC_VR;                  // knots, 0 if not set
        public byte FMC_V2;                  // knots, 0 if not set
        public ushort FMC_ThrustRedAlt;      // 1: FLAPS 1,  5: FLAPS 5,  otherwise altitude in ft
        public ushort FMC_AccelerationAlt;   // ft
        public ushort FMC_EOAccelerationAlt; // ft
        public byte FMC_LandingFlaps;        // degrees, 0 if not set
        public byte FMC_LandingVREF;         // knots, 0 if not set
        public ushort FMC_CruiseAlt;         // ft, 0 if not set
        public short FMC_LandingAltitude;    // ft; -32767 if not available
        public ushort FMC_TransitionAlt;     // ft
        public ushort FMC_TransitionLevel;   // ft
        [MarshalAs(UnmanagedType.I1)] public bool FMC_PerfInputComplete;
        public float FMC_DistanceToTOD;                    // nm; 0.0 if passed, negative if n/a
        public float FMC_DistanceToDest;                   // nm; negative if n/a
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)] public string FMC_flightNumber;
        [MarshalAs(UnmanagedType.I1)] public bool WheelChocksSet;
        [MarshalAs(UnmanagedType.I1)] public bool APURunning;

        // FMC thrust limit mode
        // Values are:
        //  0:  TO,
        //  1:  TO 1,
        //  2:  TO 2,
        //  3:  TO B,
        //  4:  CLB,
        //  5:  CLB 1,
        //  6:  CLB 2,
        //  7:  CRZ,
        //  8:  CON,
        //  9:  G/A,
        // 10:  D-TO,
        // 11:  D-TO 1,
        // 12:  D-TO 2,
        // 13:  A-TO,
        // 14:  A-TO 1,
        // 15:  A-TO 2,
        // 16:  A-TO B
        public byte FMC_ThrustLimitMode;

        // Normal checklist completion status
        // Array elements are:
        // 	0:  PREFLIGHT,
        // 	1:  BEFORE_START,
        // 	2:  BEFORE_TAXI,
        // 	3:  BEFORE_TAKEOFF,
        // 	4:  AFTER_TAKEOFF,
        // 	5:  DESCENT,
        // 	6:  APPROACH,
        // 	7:  LANDING,
        // 	8:  SHUTDOWN,
        // 	9:  SECURE

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 10)] public bool[] ECL_ChecklistComplete;

        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 84)] public byte[] reserved;
    }
    
    // ReSharper disable InconsistentNaming
    //////////////////////////////////////////////////////////////////
    //
    //  777 EVENTS 
    //
    //////////////////////////////////////////////////////////////////
    
    private const int THIRD_PARTY_EVENT_ID_MIN = 0x00011000; // equals to 69632
    private const int EVT_EFB_L_START = THIRD_PARTY_EVENT_ID_MIN + 1700;
    private const int EVT_EFB_L_KEY_START = EVT_EFB_L_START + 30;
    private const int EVT_EFB_R_START = EVT_EFB_L_KEY_START + 55;
    private const int EVT_EFB_R_KEY_START = EVT_EFB_R_START + 30;

    public enum Event
    {
        // Overhead - Hydraulic
        EVT_OH_HYD_DEMAND_ELEC1 = THIRD_PARTY_EVENT_ID_MIN + 35,
        EVT_OH_HYD_AIR1 = THIRD_PARTY_EVENT_ID_MIN + 36,
        EVT_OH_HYD_AIR2 = THIRD_PARTY_EVENT_ID_MIN + 37,
        EVT_OH_HYD_DEMAND_ELEC2 = THIRD_PARTY_EVENT_ID_MIN + 38,
        EVT_OH_HYD_ENG1 = THIRD_PARTY_EVENT_ID_MIN + 39,
        EVT_OH_HYD_ELEC1 = THIRD_PARTY_EVENT_ID_MIN + 40,
        EVT_OH_HYD_ELEC2 = THIRD_PARTY_EVENT_ID_MIN + 41,
        EVT_OH_HYD_ENG2 = THIRD_PARTY_EVENT_ID_MIN + 42,
        EVT_OH_HYD_RAM_AIR = THIRD_PARTY_EVENT_ID_MIN + 43,
        EVT_OH_HYD_RAM_AIR_COVER = THIRD_PARTY_EVENT_ID_MIN + 44,

        // Overhead - Electric  
        EVT_OH_ELEC_BATTERY_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1,
        EVT_OH_ELEC_APU_GEN_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 2,
        EVT_OH_ELEC_APU_SEL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 3,
        EVT_OH_ELEC_BUS_TIE1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 5,
        EVT_OH_ELEC_BUS_TIE2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 6,
        EVT_OH_ELEC_GRD_PWR_SEC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 7,
        EVT_OH_ELEC_GRD_PWR_PRIM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 8,
        EVT_OH_ELEC_GEN1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 9,
        EVT_OH_ELEC_GEN2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 10,
        EVT_OH_ELEC_BACKUP_GEN1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 11,
        EVT_OH_ELEC_BACKUP_GEN2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 12,
        EVT_OH_ELEC_DISCONNECT1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 13,
        EVT_OH_ELEC_DISCONNECT1_GUARD = THIRD_PARTY_EVENT_ID_MIN + 14,
        EVT_OH_ELEC_DISCONNECT2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 15,
        EVT_OH_ELEC_DISCONNECT2_GUARD = THIRD_PARTY_EVENT_ID_MIN + 16,
        EVT_OH_ELEC_IFE = THIRD_PARTY_EVENT_ID_MIN + 17,
        EVT_OH_ELEC_CAB_UTIL = THIRD_PARTY_EVENT_ID_MIN + 18,
        EVT_OH_ELEC_STBY_PWR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 81,
        EVT_OH_ELEC_STBY_PWR_GUARD = THIRD_PARTY_EVENT_ID_MIN + 82,
        EVT_OH_ELEC_TOWING_PWR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 150,
        EVT_OH_ELEC_TOWING_PWR_GUARD = THIRD_PARTY_EVENT_ID_MIN + 151,
        EVT_OH_ELEC_GND_TEST_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 152,
        EVT_OH_ELEC_GND_TEST_GUARD = THIRD_PARTY_EVENT_ID_MIN + 153,

        // Overhead - FUEL Panel
        EVT_OH_FUEL_JETTISON_NOZZLE_L = THIRD_PARTY_EVENT_ID_MIN + 97,
        EVT_OH_FUEL_JETTISON_NOZZLE_L_GUARD = THIRD_PARTY_EVENT_ID_MIN + 98,
        EVT_OH_FUEL_JETTISON_NOZZLE_R = THIRD_PARTY_EVENT_ID_MIN + 99,
        EVT_OH_FUEL_JETTISON_NOZZLE_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 100,
        EVT_OH_FUEL_TO_REMAIN_ROTATE = THIRD_PARTY_EVENT_ID_MIN + 101,
        EVT_OH_FUEL_TO_REMAIN_PULL = THIRD_PARTY_EVENT_ID_MIN + 1011,	
        EVT_OH_FUEL_JETTISON_ARM = THIRD_PARTY_EVENT_ID_MIN + 102,
        EVT_OH_FUEL_PUMP_1_FORWARD = THIRD_PARTY_EVENT_ID_MIN + 103,
        EVT_OH_FUEL_PUMP_2_FORWARD = THIRD_PARTY_EVENT_ID_MIN + 104,
        EVT_OH_FUEL_PUMP_1_AFT = THIRD_PARTY_EVENT_ID_MIN + 105,
        EVT_OH_FUEL_PUMP_2_AFT = THIRD_PARTY_EVENT_ID_MIN + 106,
        EVT_OH_FUEL_CROSSFEED_FORWARD = THIRD_PARTY_EVENT_ID_MIN + 107,
        EVT_OH_FUEL_CROSSFEED_AFT = THIRD_PARTY_EVENT_ID_MIN + 108,
        EVT_OH_FUEL_PUMP_L_CENTER = THIRD_PARTY_EVENT_ID_MIN + 109,
        EVT_OH_FUEL_PUMP_R_CENTER = THIRD_PARTY_EVENT_ID_MIN + 110,
        EVT_OH_FUEL_PUMP_AUX = THIRD_PARTY_EVENT_ID_MIN + 1037,


        // Overhead Air
        EVT_OH_BLEED_ENG_1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 129,
        EVT_OH_BLEED_ENG_2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 130,
        EVT_OH_BLEED_APU_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 131,
        EVT_OH_BLEED_ISOLATION_VALVE_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 132,
        EVT_OH_BLEED_ISOLATION_VALVE_SWITCH_C = THIRD_PARTY_EVENT_ID_MIN + 133,
        EVT_OH_BLEED_ISOLATION_VALVE_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 134,
        EVT_OH_AIRCOND_PACK_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 135,
        EVT_OH_AIRCOND_PACK_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 136,
        EVT_OH_AIRCOND_TRIM_AIR_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 137,
        EVT_OH_AIRCOND_TRIM_AIR_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 138,
        EVT_OH_AIRCOND_TEMP_SELECTOR_FLT_DECK = THIRD_PARTY_EVENT_ID_MIN + 139,
        EVT_OH_AIRCOND_TEMP_SELECTOR_CABIN = THIRD_PARTY_EVENT_ID_MIN + 140,
        EVT_OH_AIRCOND_RESET_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 141,
        EVT_OH_AIRCOND_RECIRC_FAN_UPP_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 142,
        EVT_OH_AIRCOND_RECIRC_FAN_LWR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 143,
        EVT_OH_AIRCOND_EQUIP_COOLING_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 144,
        EVT_OH_AIRCOND_GASPER_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 145,
        EVT_OH_AIRCOND_TEMP_SELECTOR_CARGO_AFT = THIRD_PARTY_EVENT_ID_MIN + 148,
        EVT_OH_AIRCOND_TEMP_SELECTOR_CARGO_BULK = THIRD_PARTY_EVENT_ID_MIN + 149,
        EVT_OH_AIRCOND_TEMP_SELECTOR_LWR_CARGO_FWD = THIRD_PARTY_EVENT_ID_MIN + 1050,
        EVT_OH_AIRCOND_TEMP_SELECTOR_LWR_CARGO_AFT = THIRD_PARTY_EVENT_ID_MIN + 1051,
        EVT_OH_AIRCOND_RECIRC_FANS_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1052,
        EVT_OH_AIRCOND_TEMP_SELECTOR_MAIN_CARGO_FWD = THIRD_PARTY_EVENT_ID_MIN + 1054,
        EVT_OH_AIRCOND_TEMP_SELECTOR_MAIN_CARGO_AFT = THIRD_PARTY_EVENT_ID_MIN + 1055,
        EVT_OH_AIRCOND_MAIN_DECK_FLOW_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1053,
        EVT_OH_AIRCOND_ALT_VENT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1057,
        EVT_OH_AIRCOND_ALT_VENT_GUARD = THIRD_PARTY_EVENT_ID_MIN + 1111,

        // Overhead - Cabin Press
        EVT_OH_PRESS_VALVE_SWITCH_MANUAL_1 = THIRD_PARTY_EVENT_ID_MIN + 124,
        EVT_OH_PRESS_VALVE_SWITCH_MANUAL_2 = THIRD_PARTY_EVENT_ID_MIN + 125,
        EVT_OH_PRESS_VALVE_SWITCHES_MANUAL = THIRD_PARTY_EVENT_ID_MIN + 1251,
        EVT_OH_PRESS_LAND_ALT_KNOB_ROTATE = THIRD_PARTY_EVENT_ID_MIN + 126,
        EVT_OH_PRESS_LAND_ALT_KNOB_PULL = THIRD_PARTY_EVENT_ID_MIN + 1261,
        EVT_OH_PRESS_VALVE_SWITCH_1 = THIRD_PARTY_EVENT_ID_MIN + 127,
        EVT_OH_PRESS_VALVE_SWITCH_2 = THIRD_PARTY_EVENT_ID_MIN + 128,

        // Overhead - ANTI-ICE
        EVT_OH_ICE_WINDOW_HEAT_1 = THIRD_PARTY_EVENT_ID_MIN + 45,
        EVT_OH_ICE_WINDOW_HEAT_2 = THIRD_PARTY_EVENT_ID_MIN + 46,
        EVT_OH_ICE_WINDOW_HEAT_3 = THIRD_PARTY_EVENT_ID_MIN + 47,
        EVT_OH_ICE_WINDOW_HEAT_4 = THIRD_PARTY_EVENT_ID_MIN + 48,
        EVT_OH_ICE_BU_WINDOW_HEAT_L = THIRD_PARTY_EVENT_ID_MIN + 77,
        EVT_OH_ICE_BU_WINDOW_HEAT_L_GUARD = THIRD_PARTY_EVENT_ID_MIN + 78,
        EVT_OH_ICE_BU_WINDOW_HEAT_R = THIRD_PARTY_EVENT_ID_MIN + 79,
        EVT_OH_ICE_BU_WINDOW_HEAT_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 80,
        EVT_OH_ICE_WING_ANTIICE = THIRD_PARTY_EVENT_ID_MIN + 111,
        EVT_OH_ICE_ENGINE_ANTIICE_1 = THIRD_PARTY_EVENT_ID_MIN + 112,
        EVT_OH_ICE_ENGINE_ANTIICE_2 = THIRD_PARTY_EVENT_ID_MIN + 113,

        // Overhead Lights Panel
        EVT_OH_LIGHTS_LANDING_L = THIRD_PARTY_EVENT_ID_MIN + 22,
        EVT_OH_LIGHTS_LANDING_NOSE = THIRD_PARTY_EVENT_ID_MIN + 23,
        EVT_OH_LIGHTS_LANDING_R = THIRD_PARTY_EVENT_ID_MIN + 24,
        EVT_OH_LIGHTS_LANDING_LNR = THIRD_PARTY_EVENT_ID_MIN + 2341,
        EVT_OH_LIGHTS_STORM = THIRD_PARTY_EVENT_ID_MIN + 27,
        EVT_OH_LIGHTS_BEACON = THIRD_PARTY_EVENT_ID_MIN + 114,
        EVT_OH_LIGHTS_NAV = THIRD_PARTY_EVENT_ID_MIN + 115,
        EVT_OH_LIGHTS_LOGO = THIRD_PARTY_EVENT_ID_MIN + 116,
        EVT_OH_LIGHTS_WING = THIRD_PARTY_EVENT_ID_MIN + 117,
        EVT_OH_LIGHTS_IND_LTS_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 118,
        EVT_OH_LIGHTS_L_TURNOFF = THIRD_PARTY_EVENT_ID_MIN + 119,
        EVT_OH_LIGHTS_R_TURNOFF = THIRD_PARTY_EVENT_ID_MIN + 120,
        EVT_OH_LIGHTS_LR_TURNOFF = THIRD_PARTY_EVENT_ID_MIN + 1201,
        EVT_OH_LIGHTS_TAXI = THIRD_PARTY_EVENT_ID_MIN + 121,
        EVT_OH_LIGHTS_STROBE = THIRD_PARTY_EVENT_ID_MIN + 122,
        EVT_OH_NO_SMOKING_LIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 29,
        EVT_OH_FASTEN_BELTS_LIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 30,
        EVT_OH_PANEL_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 25,
        EVT_OH_CB_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 2501,
        EVT_OH_GS_PANEL_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 21,
        EVT_OH_GS_FLOOD_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 2101,
        EVT_OH_DOME_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 26,
        EVT_OH_MASTER_BRIGHT_ROTATE = THIRD_PARTY_EVENT_ID_MIN + 28,
        EVT_OH_MASTER_BRIGHT_PUSH = THIRD_PARTY_EVENT_ID_MIN + 2801,

        // Overhead - APU & Cargo Fire Panel
        EVT_OH_FIRE_CARGO_ARM_FWD = THIRD_PARTY_EVENT_ID_MIN + 85,
        EVT_OH_FIRE_CARGO_ARM_AFT = THIRD_PARTY_EVENT_ID_MIN + 86,
        EVT_OH_FIRE_CARGO_DISCH = THIRD_PARTY_EVENT_ID_MIN + 87,
        EVT_OH_FIRE_CARGO_DISCH_GUARD = THIRD_PARTY_EVENT_ID_MIN + 88,
        EVT_OH_FIRE_OVHT_TEST = THIRD_PARTY_EVENT_ID_MIN + 89,
        EVT_OH_FIRE_HANDLE_APU_TOP = THIRD_PARTY_EVENT_ID_MIN + 84,
        EVT_OH_FIRE_HANDLE_APU_BOTTOM = THIRD_PARTY_EVENT_ID_MIN + 8401,
        EVT_OH_FIRE_UNLOCK_SWITCH_APU = THIRD_PARTY_EVENT_ID_MIN + 8402,
        EVT_OH_FIRE_CARGO_ARM_MAIN_DECK = THIRD_PARTY_EVENT_ID_MIN + 1074,
        EVT_OH_FIRE_CARGO_DISCH_DEPR = THIRD_PARTY_EVENT_ID_MIN + 1075,

        // Overhead - Engine control
        EVT_OH_EEC_L_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 90,
        EVT_OH_EEC_L_GUARD = THIRD_PARTY_EVENT_ID_MIN + 91,
        EVT_OH_EEC_R_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 92,
        EVT_OH_EEC_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 93,
        EVT_OH_ENGINE_L_START = THIRD_PARTY_EVENT_ID_MIN + 94,
        EVT_OH_ENGINE_R_START = THIRD_PARTY_EVENT_ID_MIN + 95,
        EVT_OH_ENGINE_AUTOSTART = THIRD_PARTY_EVENT_ID_MIN + 96,

        // Overhead - Miscellaneous
        EVT_OH_CAMERA_LTS_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 19,
        EVT_OH_WIPER_LEFT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 20,
        EVT_OH_EMER_EXIT_LIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 49,
        EVT_OH_EMER_EXIT_LIGHT_GUARD = THIRD_PARTY_EVENT_ID_MIN + 50,
        EVT_OH_SERVICE_INTERPHONE_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 51,
        EVT_OH_OXY_PASS_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 52,
        EVT_OH_OXY_PASS_GUARD = THIRD_PARTY_EVENT_ID_MIN + 53,
        EVT_OH_OXY_SUPRNMRY_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1076,
        EVT_OH_OXY_SUPRNMRY_GUARD = THIRD_PARTY_EVENT_ID_MIN + 1077,
        EVT_OH_THRUST_ASYM_COMP = THIRD_PARTY_EVENT_ID_MIN + 54,
        EVT_OH_PRIM_FLT_COMPUTERS = THIRD_PARTY_EVENT_ID_MIN + 55,
        EVT_OH_PRIM_FLT_COMPUTERS_GUARD = THIRD_PARTY_EVENT_ID_MIN + 56,
        EVT_OH_ADIRU_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 59,
        EVT_OH_HYD_VLV_PWR_WING_L = THIRD_PARTY_EVENT_ID_MIN + 60,
        EVT_OH_HYD_VLV_PWR_WING_L_GUARD = THIRD_PARTY_EVENT_ID_MIN + 61,
        EVT_OH_HYD_VLV_PWR_WING_C = THIRD_PARTY_EVENT_ID_MIN + 63,
        EVT_OH_HYD_VLV_PWR_WING_C_GUARD = THIRD_PARTY_EVENT_ID_MIN + 64,
        EVT_OH_HYD_VLV_PWR_WING_R = THIRD_PARTY_EVENT_ID_MIN + 66,
        EVT_OH_HYD_VLV_PWR_WING_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 67,
        EVT_OH_HYD_VLV_PWR_TAIL_L = THIRD_PARTY_EVENT_ID_MIN + 69,
        EVT_OH_HYD_VLV_PWR_TAIL_L_GUARD = THIRD_PARTY_EVENT_ID_MIN + 70,
        EVT_OH_HYD_VLV_PWR_TAIL_C = THIRD_PARTY_EVENT_ID_MIN + 71,
        EVT_OH_HYD_VLV_PWR_TAIL_C_GUARD = THIRD_PARTY_EVENT_ID_MIN + 72,
        EVT_OH_HYD_VLV_PWR_TAIL_R = THIRD_PARTY_EVENT_ID_MIN + 74,
        EVT_OH_HYD_VLV_PWR_TAIL_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 75,
        EVT_OH_WIPER_RIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 123,
        EVT_OH_CVR_TEST = THIRD_PARTY_EVENT_ID_MIN + 156,
        EVT_OH_CVR_ERASE = THIRD_PARTY_EVENT_ID_MIN + 157,
        EVT_OH_APU_TEST_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 159,
        EVT_OH_APU_TEST_SWITCH_GUARD = THIRD_PARTY_EVENT_ID_MIN + 160,
        EVT_OH_EEC_TEST_L_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 161,
        EVT_OH_EEC_TEST_L_SWITCH_GUARD = THIRD_PARTY_EVENT_ID_MIN + 162,
        EVT_OH_EEC_TEST_R_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 163,
        EVT_OH_EEC_TEST_R_SWITCH_GUARD = THIRD_PARTY_EVENT_ID_MIN + 164,
        EVT_GPWS_RWY_OVRD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1109,
        EVT_GPWS_RWY_OVRD_GUARD = THIRD_PARTY_EVENT_ID_MIN + 1110,

        // Forward Panel - Instrument Source Select
        EVT_FWD_NAV_SOURCE_L = THIRD_PARTY_EVENT_ID_MIN + 168,
        EVT_FWD_DSPL_CTRL_SOURCE_L = THIRD_PARTY_EVENT_ID_MIN + 169,
        EVT_FWD_AIR_DATA_ATT_SOURCE_L = THIRD_PARTY_EVENT_ID_MIN + 170,
        EVT_FWD_NAV_SOURCE_R = THIRD_PARTY_EVENT_ID_MIN + 276,
        EVT_FWD_DSPL_CTRL_SOURCE_R = THIRD_PARTY_EVENT_ID_MIN + 277,
        EVT_FWD_AIR_DATA_ATT_SOURCE_R = THIRD_PARTY_EVENT_ID_MIN + 278,
        EVT_FWD_FMC_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 291,

        // Forward Panel - Display Selectors
        EVT_DSP_INDB_DSPL_L = THIRD_PARTY_EVENT_ID_MIN + 315,
        EVT_DSP_INDB_DSPL_R = THIRD_PARTY_EVENT_ID_MIN + 290,

        // Forward Panel - Heading Reference
        EVT_EFIS_HDG_REF_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 313,
        EVT_EFIS_HDG_REF_GUARD = THIRD_PARTY_EVENT_ID_MIN + 314,

        // Forward Panel - Gear
        EVT_GEAR_LEVER = THIRD_PARTY_EVENT_ID_MIN + 295,
        EVT_GEAR_LEVER_UNLOCK = THIRD_PARTY_EVENT_ID_MIN + 296,
        EVT_GEAR_ALTN_GEAR_DOWN = THIRD_PARTY_EVENT_ID_MIN + 293,
        EVT_GEAR_ALTN_GEAR_DOWN_GUARD = THIRD_PARTY_EVENT_ID_MIN + 294,

        // Forward Panel - GPWS
        EVT_GPWS_TERR_OVRD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 297,
        EVT_GPWS_TERR_OVRD_GUARD = THIRD_PARTY_EVENT_ID_MIN + 298,
        EVT_GPWS_GEAR_OVRD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 299,
        EVT_GPWS_GEAR_OVRD_GUARD = THIRD_PARTY_EVENT_ID_MIN + 300,
        EVT_GPWS_FLAP_OVRD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 301,
        EVT_GPWS_FLAP_OVRD_GUARD = THIRD_PARTY_EVENT_ID_MIN + 302,
        EVT_GPWS_GS_INHIBIT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 303,

        // Forward Panel - Autobrakes
        EVT_ABS_AUTOBRAKE_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 292,

        // Forward Panel - ISFD
        EVT_ISFD_APP = THIRD_PARTY_EVENT_ID_MIN + 810,
        EVT_ISFD_HP_IN = THIRD_PARTY_EVENT_ID_MIN + 811,
        EVT_ISFD_PLUS = THIRD_PARTY_EVENT_ID_MIN + 812,
        EVT_ISFD_MINUS = THIRD_PARTY_EVENT_ID_MIN + 813,
        EVT_ISFD_ATT_RST = THIRD_PARTY_EVENT_ID_MIN + 814,
        EVT_ISFD_BARO = THIRD_PARTY_EVENT_ID_MIN + 815,
        EVT_ISFD_BARO_PUSH = THIRD_PARTY_EVENT_ID_MIN + 816,

        // Forward Panel - non-ISFD standby instruments
        EVT_STANDBY_ASI_KNOB = THIRD_PARTY_EVENT_ID_MIN + 308,
        EVT_STANDBY_ASI_KNOB_PUSH = THIRD_PARTY_EVENT_ID_MIN + 3080,
        EVT_STANDBY_ALTIMETER_KNOB = THIRD_PARTY_EVENT_ID_MIN + 311,
        EVT_STANDBY_ALTIMETER_KNOB_PUSH = THIRD_PARTY_EVENT_ID_MIN + 3110,

        // Forward Panel - Chronometers
        EVT_CHRONO_L_CHR = THIRD_PARTY_EVENT_ID_MIN + 171,
        EVT_CHRONO_L_TIME_DATE_SELECT = THIRD_PARTY_EVENT_ID_MIN + 172,
        EVT_CHRONO_L_TIME_DATE_PUSH = THIRD_PARTY_EVENT_ID_MIN + 1721,
        EVT_CHRONO_L_ET = THIRD_PARTY_EVENT_ID_MIN + 173,
        EVT_CHRONO_L_SET = THIRD_PARTY_EVENT_ID_MIN + 174,
        EVT_CHRONO_R_CHR = THIRD_PARTY_EVENT_ID_MIN + 279,
        EVT_CHRONO_R_TIME_DATE_SELECT = THIRD_PARTY_EVENT_ID_MIN + 280,
        EVT_CHRONO_R_TIME_DATE_PUSH = THIRD_PARTY_EVENT_ID_MIN + 2802,
        EVT_CHRONO_R_ET = THIRD_PARTY_EVENT_ID_MIN + 281,
        EVT_CHRONO_R_SET = THIRD_PARTY_EVENT_ID_MIN + 282,

        // Forward Panel - Left Sidewall
        EVT_FWD_LEFT_SHOULDER_HEATER = THIRD_PARTY_EVENT_ID_MIN + 318,
        EVT_FWD_LEFT_FOOT_HEATER = THIRD_PARTY_EVENT_ID_MIN + 319,
        EVT_FWD_LEFT_OUTBD_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 320,
        EVT_FWD_LEFT_INBD_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 321,
        EVT_FWD_LEFT_INBD_TERR_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 3210,
        EVT_FWD_LEFT_PANEL_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 322,
        EVT_FWD_LEFT_FLOOD_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 3220,

        // Forward Panel - Right Sidewall
        EVT_FWD_RIGHT_FOOT_HEATER = THIRD_PARTY_EVENT_ID_MIN + 288,
        EVT_FWD_RIGHT_SHOULDER_HEATER = THIRD_PARTY_EVENT_ID_MIN + 289,
        EVT_FWD_RIGHT_PANEL_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 285,
        EVT_FWD_RIGHT_FLOOD_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 2850,
        EVT_FWD_RIGHT_INBD_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 286,
        EVT_FWD_RIGHT_INBD_TERR_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 2860,
        EVT_FWD_RIGHT_OUTBD_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 287,

        // Glareshield - EFIS Captain control panel
        EVT_EFIS_CPT_MINIMUMS_RADIO_BARO = THIRD_PARTY_EVENT_ID_MIN + 181,

        //EVT_EFIS_CPT_FIRST = EVT_EFIS_CPT_MINIMUMS_RADIO_BARO,
        EVT_EFIS_CPT_MINIMUMS = THIRD_PARTY_EVENT_ID_MIN + 182,
        EVT_EFIS_CPT_MINIMUMS_RST = THIRD_PARTY_EVENT_ID_MIN + 183,
        EVT_EFIS_CPT_VOR_ADF_SELECTOR_L = THIRD_PARTY_EVENT_ID_MIN + 184,
        EVT_EFIS_CPT_MODE = THIRD_PARTY_EVENT_ID_MIN + 185,
        EVT_EFIS_CPT_MODE_CTR = THIRD_PARTY_EVENT_ID_MIN + 186,
        EVT_EFIS_CPT_RANGE = THIRD_PARTY_EVENT_ID_MIN + 187,
        EVT_EFIS_CPT_RANGE_TFC = THIRD_PARTY_EVENT_ID_MIN + 188,
        EVT_EFIS_CPT_VOR_ADF_SELECTOR_R = THIRD_PARTY_EVENT_ID_MIN + 189,
        EVT_EFIS_CPT_BARO_IN_HPA = THIRD_PARTY_EVENT_ID_MIN + 190,
        EVT_EFIS_CPT_BARO = THIRD_PARTY_EVENT_ID_MIN + 191,
        EVT_EFIS_CPT_BARO_STD = THIRD_PARTY_EVENT_ID_MIN + 192,
        EVT_EFIS_CPT_FPV = THIRD_PARTY_EVENT_ID_MIN + 193,
        EVT_EFIS_CPT_MTRS = THIRD_PARTY_EVENT_ID_MIN + 194,
        EVT_EFIS_CPT_WXR = THIRD_PARTY_EVENT_ID_MIN + 195,
        EVT_EFIS_CPT_STA = THIRD_PARTY_EVENT_ID_MIN + 196,
        EVT_EFIS_CPT_WPT = THIRD_PARTY_EVENT_ID_MIN + 197,
        EVT_EFIS_CPT_ARPT = THIRD_PARTY_EVENT_ID_MIN + 198,
        EVT_EFIS_CPT_DATA = THIRD_PARTY_EVENT_ID_MIN + 199,
        EVT_EFIS_CPT_POS = THIRD_PARTY_EVENT_ID_MIN + 200,
        EVT_EFIS_CPT_TERR = THIRD_PARTY_EVENT_ID_MIN + 201,
        //EVT_EFIS_CPT_LAST = EVT_EFIS_CPT_TERR,

        // Glareshield - EFIS F/O control panels
        EVT_EFIS_FO_MINIMUMS_RADIO_BARO = THIRD_PARTY_EVENT_ID_MIN + 248,

        //EVT_EFIS_FO_FIRST = EVT_EFIS_FO_MINIMUMS_RADIO_BARO,
        EVT_EFIS_FO_MINIMUMS = THIRD_PARTY_EVENT_ID_MIN + 249,
        EVT_EFIS_FO_MINIMUMS_RST = THIRD_PARTY_EVENT_ID_MIN + 250,
        EVT_EFIS_FO_VOR_ADF_SELECTOR_L = THIRD_PARTY_EVENT_ID_MIN + 251,
        EVT_EFIS_FO_MODE = THIRD_PARTY_EVENT_ID_MIN + 252,
        EVT_EFIS_FO_MODE_CTR = THIRD_PARTY_EVENT_ID_MIN + 253,
        EVT_EFIS_FO_RANGE = THIRD_PARTY_EVENT_ID_MIN + 254,
        EVT_EFIS_FO_RANGE_TFC = THIRD_PARTY_EVENT_ID_MIN + 255,
        EVT_EFIS_FO_VOR_ADF_SELECTOR_R = THIRD_PARTY_EVENT_ID_MIN + 256,
        EVT_EFIS_FO_BARO_IN_HPA = THIRD_PARTY_EVENT_ID_MIN + 257,
        EVT_EFIS_FO_BARO = THIRD_PARTY_EVENT_ID_MIN + 258,
        EVT_EFIS_FO_BARO_STD = THIRD_PARTY_EVENT_ID_MIN + 259,
        EVT_EFIS_FO_FPV = THIRD_PARTY_EVENT_ID_MIN + 260,
        EVT_EFIS_FO_MTRS = THIRD_PARTY_EVENT_ID_MIN + 261,
        EVT_EFIS_FO_WXR = THIRD_PARTY_EVENT_ID_MIN + 262,
        EVT_EFIS_FO_STA = THIRD_PARTY_EVENT_ID_MIN + 263,
        EVT_EFIS_FO_WPT = THIRD_PARTY_EVENT_ID_MIN + 264,
        EVT_EFIS_FO_ARPT = THIRD_PARTY_EVENT_ID_MIN + 265,
        EVT_EFIS_FO_DATA = THIRD_PARTY_EVENT_ID_MIN + 2661,
        EVT_EFIS_FO_POS = THIRD_PARTY_EVENT_ID_MIN + 267,
        EVT_EFIS_FO_TERR = THIRD_PARTY_EVENT_ID_MIN + 268,
        //EVT_EFIS_FO_LAST = EVT_EFIS_FO_TERR,

        // Glareshield - MCP
        EVT_MCP_FD_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 202,
        EVT_MCP_AP_L_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 203,
        EVT_MCP_AT_ARM_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 204,
        EVT_MCP_AT_ARM_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 205,
        EVT_MCP_CLB_CON_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 206,
        EVT_MCP_AT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 207,
        EVT_MCP_IAS_MACH_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 208,
        EVT_MCP_SPEED_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 210,
        EVT_MCP_SPEED_PUSH_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 2100,
        EVT_MCP_LNAV_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 211,
        EVT_MCP_VNAV_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 212,
        EVT_MCP_LVL_CHG_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 213,
        EVT_MCP_DISENGAGE_BAR = THIRD_PARTY_EVENT_ID_MIN + 214,
        EVT_MCP_HDG_TRK_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 216,
        EVT_MCP_HEADING_PUSH_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 218,
        EVT_MCP_HEADING_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 2180,
        EVT_MCP_BANK_ANGLE_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 2181,
        EVT_MCP_HDG_HOLD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 219,
        EVT_MCP_VS_FPA_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 220,
        EVT_MCP_VS_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 222,
        EVT_MCP_VS_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 223,
        EVT_MCP_ALTITUDE_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 2250,
        EVT_MCP_ALTITUDE_PUSH_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 2251,
        EVT_MCP_ALT_INCR_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 225,
        EVT_MCP_ALT_HOLD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 226,
        EVT_MCP_LOC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 227,
        EVT_MCP_APP_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 228,
        EVT_MCP_AP_R_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 229,
        EVT_MCP_FD_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 230,
        EVT_MCP_TOGA_SCREW_L = THIRD_PARTY_EVENT_ID_MIN + 5001,
        EVT_MCP_TOGA_SCREW_R = THIRD_PARTY_EVENT_ID_MIN + 5002,

        // Glareshield - Display Select Panel
        EVT_DSP_L_INBD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 231,
        EVT_DSP_R_INBD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 232,
        EVT_DSP_LWR_CTR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 233,
        EVT_DSP_ENG_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 234,
        EVT_DSP_STAT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 235,
        EVT_DSP_ELEC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 236,
        EVT_DSP_HYD_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 237,
        EVT_DSP_FUEL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 238,
        EVT_DSP_AIR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 239,
        EVT_DSP_DOOR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 240,
        EVT_DSP_GEAR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 241,
        EVT_DSP_FCTL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 242,
        EVT_DSP_CAM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 243,
        EVT_DSP_CHKL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 244,
        EVT_DSP_COMM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 245,
        EVT_DSP_NAV_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 246,
        EVT_DSP_CANC_RCL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 247,

        // Glareshield - Master Warning/caution
        EVT_MASTER_WARNING_RESET_LEFT = THIRD_PARTY_EVENT_ID_MIN + 177,
        EVT_MASTER_WARNING_RESET_RIGHT = THIRD_PARTY_EVENT_ID_MIN + 272,

        // Glareshield - Data Link Switches
        EVT_DATA_LINK_ACPT_LEFT = THIRD_PARTY_EVENT_ID_MIN + 178,
        EVT_DATA_LINK_CANC_LEFT = THIRD_PARTY_EVENT_ID_MIN + 179,
        EVT_DATA_LINK_RJCT_LEFT = THIRD_PARTY_EVENT_ID_MIN + 180,
        EVT_DATA_LINK_ACPT_RIGHT = THIRD_PARTY_EVENT_ID_MIN + 269,
        EVT_DATA_LINK_CANC_RIGHT = THIRD_PARTY_EVENT_ID_MIN + 270,
        EVT_DATA_LINK_RJCT_RIGHT = THIRD_PARTY_EVENT_ID_MIN + 271,


        // Glareshield - Map/Chart/Worktable Lights, MIC & Clock Switches
        EVT_CLOCK_L = THIRD_PARTY_EVENT_ID_MIN + 165,
        EVT_MAP_LIGHT_L = THIRD_PARTY_EVENT_ID_MIN + 166,
        EVT_MAP_LIGHT_L_PULL = THIRD_PARTY_EVENT_ID_MIN + 1661,
        EVT_GLARESHIELD_MIC_L = THIRD_PARTY_EVENT_ID_MIN + 167,
        EVT_GLARESHIELD_MIC_R = THIRD_PARTY_EVENT_ID_MIN + 273,
        EVT_MAP_LIGHT_R = THIRD_PARTY_EVENT_ID_MIN + 274,
        EVT_MAP_LIGHT_R_PULL = THIRD_PARTY_EVENT_ID_MIN + 2741,
        EVT_CLOCK_R = THIRD_PARTY_EVENT_ID_MIN + 275,
        EVT_CHART_LIGHT_L = THIRD_PARTY_EVENT_ID_MIN + 1107,
        EVT_CHART_LIGHT_R = THIRD_PARTY_EVENT_ID_MIN + 1108,
        EVT_WORKTABLE_LIGHT_L = THIRD_PARTY_EVENT_ID_MIN + 1105,
        EVT_WORKTABLE_LIGHT_R = THIRD_PARTY_EVENT_ID_MIN + 1106,

        // Pedestal - Fwd Aisle Stand - CDUs
        EVT_CDU_L_L1 = THIRD_PARTY_EVENT_ID_MIN + 328,
        EVT_CDU_L_L2 = THIRD_PARTY_EVENT_ID_MIN + 329,
        EVT_CDU_L_L3 = THIRD_PARTY_EVENT_ID_MIN + 330,
        EVT_CDU_L_L4 = THIRD_PARTY_EVENT_ID_MIN + 331,
        EVT_CDU_L_L5 = THIRD_PARTY_EVENT_ID_MIN + 332,
        EVT_CDU_L_L6 = THIRD_PARTY_EVENT_ID_MIN + 333,
        EVT_CDU_L_R1 = THIRD_PARTY_EVENT_ID_MIN + 334,
        EVT_CDU_L_R2 = THIRD_PARTY_EVENT_ID_MIN + 335,
        EVT_CDU_L_R3 = THIRD_PARTY_EVENT_ID_MIN + 336,
        EVT_CDU_L_R4 = THIRD_PARTY_EVENT_ID_MIN + 337,
        EVT_CDU_L_R5 = THIRD_PARTY_EVENT_ID_MIN + 338,
        EVT_CDU_L_R6 = THIRD_PARTY_EVENT_ID_MIN + 339,
        EVT_CDU_L_INIT_REF = THIRD_PARTY_EVENT_ID_MIN + 340,
        EVT_CDU_L_RTE = THIRD_PARTY_EVENT_ID_MIN + 341,
        EVT_CDU_L_DEP_ARR = THIRD_PARTY_EVENT_ID_MIN + 342,
        EVT_CDU_L_ALTN = THIRD_PARTY_EVENT_ID_MIN + 343,
        EVT_CDU_L_VNAV = THIRD_PARTY_EVENT_ID_MIN + 344,
        EVT_CDU_L_FIX = THIRD_PARTY_EVENT_ID_MIN + 345,
        EVT_CDU_L_LEGS = THIRD_PARTY_EVENT_ID_MIN + 346,
        EVT_CDU_L_HOLD = THIRD_PARTY_EVENT_ID_MIN + 347,
        EVT_CDU_L_FMCCOMM = THIRD_PARTY_EVENT_ID_MIN + 3471,
        EVT_CDU_L_PROG = THIRD_PARTY_EVENT_ID_MIN + 348,
        EVT_CDU_L_EXEC = THIRD_PARTY_EVENT_ID_MIN + 349,
        EVT_CDU_L_MENU = THIRD_PARTY_EVENT_ID_MIN + 350,
        EVT_CDU_L_NAV_RAD = THIRD_PARTY_EVENT_ID_MIN + 351,
        EVT_CDU_L_PREV_PAGE = THIRD_PARTY_EVENT_ID_MIN + 352,
        EVT_CDU_L_NEXT_PAGE = THIRD_PARTY_EVENT_ID_MIN + 353,
        EVT_CDU_L_1 = THIRD_PARTY_EVENT_ID_MIN + 354,
        EVT_CDU_L_2 = THIRD_PARTY_EVENT_ID_MIN + 355,
        EVT_CDU_L_3 = THIRD_PARTY_EVENT_ID_MIN + 356,
        EVT_CDU_L_4 = THIRD_PARTY_EVENT_ID_MIN + 357,
        EVT_CDU_L_5 = THIRD_PARTY_EVENT_ID_MIN + 358,
        EVT_CDU_L_6 = THIRD_PARTY_EVENT_ID_MIN + 359,
        EVT_CDU_L_7 = THIRD_PARTY_EVENT_ID_MIN + 360,
        EVT_CDU_L_8 = THIRD_PARTY_EVENT_ID_MIN + 361,
        EVT_CDU_L_9 = THIRD_PARTY_EVENT_ID_MIN + 362,
        EVT_CDU_L_DOT = THIRD_PARTY_EVENT_ID_MIN + 363,
        EVT_CDU_L_0 = THIRD_PARTY_EVENT_ID_MIN + 364,
        EVT_CDU_L_PLUS_MINUS = THIRD_PARTY_EVENT_ID_MIN + 365,
        EVT_CDU_L_A = THIRD_PARTY_EVENT_ID_MIN + 366,
        EVT_CDU_L_B = THIRD_PARTY_EVENT_ID_MIN + 367,
        EVT_CDU_L_C = THIRD_PARTY_EVENT_ID_MIN + 368,
        EVT_CDU_L_D = THIRD_PARTY_EVENT_ID_MIN + 369,
        EVT_CDU_L_E = THIRD_PARTY_EVENT_ID_MIN + 370,
        EVT_CDU_L_F = THIRD_PARTY_EVENT_ID_MIN + 371,
        EVT_CDU_L_G = THIRD_PARTY_EVENT_ID_MIN + 372,
        EVT_CDU_L_H = THIRD_PARTY_EVENT_ID_MIN + 373,
        EVT_CDU_L_I = THIRD_PARTY_EVENT_ID_MIN + 374,
        EVT_CDU_L_J = THIRD_PARTY_EVENT_ID_MIN + 375,
        EVT_CDU_L_K = THIRD_PARTY_EVENT_ID_MIN + 376,
        EVT_CDU_L_L = THIRD_PARTY_EVENT_ID_MIN + 377,
        EVT_CDU_L_M = THIRD_PARTY_EVENT_ID_MIN + 378,
        EVT_CDU_L_N = THIRD_PARTY_EVENT_ID_MIN + 379,
        EVT_CDU_L_O = THIRD_PARTY_EVENT_ID_MIN + 380,
        EVT_CDU_L_P = THIRD_PARTY_EVENT_ID_MIN + 381,
        EVT_CDU_L_Q = THIRD_PARTY_EVENT_ID_MIN + 382,
        EVT_CDU_L_R = THIRD_PARTY_EVENT_ID_MIN + 383,
        EVT_CDU_L_S = THIRD_PARTY_EVENT_ID_MIN + 384,
        EVT_CDU_L_T = THIRD_PARTY_EVENT_ID_MIN + 385,
        EVT_CDU_L_U = THIRD_PARTY_EVENT_ID_MIN + 386,
        EVT_CDU_L_V = THIRD_PARTY_EVENT_ID_MIN + 387,
        EVT_CDU_L_W = THIRD_PARTY_EVENT_ID_MIN + 388,
        EVT_CDU_L_X = THIRD_PARTY_EVENT_ID_MIN + 389,
        EVT_CDU_L_Y = THIRD_PARTY_EVENT_ID_MIN + 390,
        EVT_CDU_L_Z = THIRD_PARTY_EVENT_ID_MIN + 391,
        EVT_CDU_L_SPACE = THIRD_PARTY_EVENT_ID_MIN + 392,
        EVT_CDU_L_DEL = THIRD_PARTY_EVENT_ID_MIN + 393,
        EVT_CDU_L_SLASH = THIRD_PARTY_EVENT_ID_MIN + 394,
        EVT_CDU_L_CLR = THIRD_PARTY_EVENT_ID_MIN + 395,
        EVT_CDU_L_BRITENESS = THIRD_PARTY_EVENT_ID_MIN + 400,

        EVT_CDU_R_L1 = THIRD_PARTY_EVENT_ID_MIN + 401,
        CDU_EVT_OFFSET_R = EVT_CDU_R_L1 - EVT_CDU_L_L1,
        EVT_CDU_R_L2 = CDU_EVT_OFFSET_R + EVT_CDU_L_L2,
        EVT_CDU_R_L3 = CDU_EVT_OFFSET_R + EVT_CDU_L_L3,
        EVT_CDU_R_L4 = CDU_EVT_OFFSET_R + EVT_CDU_L_L4,
        EVT_CDU_R_L5 = CDU_EVT_OFFSET_R + EVT_CDU_L_L5,
        EVT_CDU_R_L6 = CDU_EVT_OFFSET_R + EVT_CDU_L_L6,
        EVT_CDU_R_R1 = CDU_EVT_OFFSET_R + EVT_CDU_L_R1,
        EVT_CDU_R_R2 = CDU_EVT_OFFSET_R + EVT_CDU_L_R2,
        EVT_CDU_R_R3 = CDU_EVT_OFFSET_R + EVT_CDU_L_R3,
        EVT_CDU_R_R4 = CDU_EVT_OFFSET_R + EVT_CDU_L_R4,
        EVT_CDU_R_R5 = CDU_EVT_OFFSET_R + EVT_CDU_L_R5,
        EVT_CDU_R_R6 = CDU_EVT_OFFSET_R + EVT_CDU_L_R6,
        EVT_CDU_R_INIT_REF = CDU_EVT_OFFSET_R + EVT_CDU_L_INIT_REF,
        EVT_CDU_R_RTE = CDU_EVT_OFFSET_R + EVT_CDU_L_RTE,
        EVT_CDU_R_DEP_ARR = CDU_EVT_OFFSET_R + EVT_CDU_L_DEP_ARR,
        EVT_CDU_R_ALTN = CDU_EVT_OFFSET_R + EVT_CDU_L_ALTN,
        EVT_CDU_R_VNAV = CDU_EVT_OFFSET_R + EVT_CDU_L_VNAV,
        EVT_CDU_R_FIX = CDU_EVT_OFFSET_R + EVT_CDU_L_FIX,
        EVT_CDU_R_LEGS = CDU_EVT_OFFSET_R + EVT_CDU_L_LEGS,
        EVT_CDU_R_HOLD = CDU_EVT_OFFSET_R + EVT_CDU_L_HOLD,
        EVT_CDU_R_FMCCOMM = THIRD_PARTY_EVENT_ID_MIN + 4201,
        EVT_CDU_R_PROG = CDU_EVT_OFFSET_R + EVT_CDU_L_PROG,
        EVT_CDU_R_EXEC = CDU_EVT_OFFSET_R + EVT_CDU_L_EXEC,
        EVT_CDU_R_MENU = CDU_EVT_OFFSET_R + EVT_CDU_L_MENU,
        EVT_CDU_R_NAV_RAD = CDU_EVT_OFFSET_R + EVT_CDU_L_NAV_RAD,
        EVT_CDU_R_PREV_PAGE = CDU_EVT_OFFSET_R + EVT_CDU_L_PREV_PAGE,
        EVT_CDU_R_NEXT_PAGE = CDU_EVT_OFFSET_R + EVT_CDU_L_NEXT_PAGE,
        EVT_CDU_R_1 = CDU_EVT_OFFSET_R + EVT_CDU_L_1,
        EVT_CDU_R_2 = CDU_EVT_OFFSET_R + EVT_CDU_L_2,
        EVT_CDU_R_3 = CDU_EVT_OFFSET_R + EVT_CDU_L_3,
        EVT_CDU_R_4 = CDU_EVT_OFFSET_R + EVT_CDU_L_4,
        EVT_CDU_R_5 = CDU_EVT_OFFSET_R + EVT_CDU_L_5,
        EVT_CDU_R_6 = CDU_EVT_OFFSET_R + EVT_CDU_L_6,
        EVT_CDU_R_7 = CDU_EVT_OFFSET_R + EVT_CDU_L_7,
        EVT_CDU_R_8 = CDU_EVT_OFFSET_R + EVT_CDU_L_8,
        EVT_CDU_R_9 = CDU_EVT_OFFSET_R + EVT_CDU_L_9,
        EVT_CDU_R_DOT = CDU_EVT_OFFSET_R + EVT_CDU_L_DOT,
        EVT_CDU_R_0 = CDU_EVT_OFFSET_R + EVT_CDU_L_0,
        EVT_CDU_R_PLUS_MINUS = CDU_EVT_OFFSET_R + EVT_CDU_L_PLUS_MINUS,
        EVT_CDU_R_A = CDU_EVT_OFFSET_R + EVT_CDU_L_A,
        EVT_CDU_R_B = CDU_EVT_OFFSET_R + EVT_CDU_L_B,
        EVT_CDU_R_C = CDU_EVT_OFFSET_R + EVT_CDU_L_C,
        EVT_CDU_R_D = CDU_EVT_OFFSET_R + EVT_CDU_L_D,
        EVT_CDU_R_E = CDU_EVT_OFFSET_R + EVT_CDU_L_E,
        EVT_CDU_R_F = CDU_EVT_OFFSET_R + EVT_CDU_L_F,
        EVT_CDU_R_G = CDU_EVT_OFFSET_R + EVT_CDU_L_G,
        EVT_CDU_R_H = CDU_EVT_OFFSET_R + EVT_CDU_L_H,
        EVT_CDU_R_I = CDU_EVT_OFFSET_R + EVT_CDU_L_I,
        EVT_CDU_R_J = CDU_EVT_OFFSET_R + EVT_CDU_L_J,
        EVT_CDU_R_K = CDU_EVT_OFFSET_R + EVT_CDU_L_K,
        EVT_CDU_R_L = CDU_EVT_OFFSET_R + EVT_CDU_L_L,
        EVT_CDU_R_M = CDU_EVT_OFFSET_R + EVT_CDU_L_M,
        EVT_CDU_R_N = CDU_EVT_OFFSET_R + EVT_CDU_L_N,
        EVT_CDU_R_O = CDU_EVT_OFFSET_R + EVT_CDU_L_O,
        EVT_CDU_R_P = CDU_EVT_OFFSET_R + EVT_CDU_L_P,
        EVT_CDU_R_Q = CDU_EVT_OFFSET_R + EVT_CDU_L_Q,
        EVT_CDU_R_R = CDU_EVT_OFFSET_R + EVT_CDU_L_R,
        EVT_CDU_R_S = CDU_EVT_OFFSET_R + EVT_CDU_L_S,
        EVT_CDU_R_T = CDU_EVT_OFFSET_R + EVT_CDU_L_T,
        EVT_CDU_R_U = CDU_EVT_OFFSET_R + EVT_CDU_L_U,
        EVT_CDU_R_V = CDU_EVT_OFFSET_R + EVT_CDU_L_V,
        EVT_CDU_R_W = CDU_EVT_OFFSET_R + EVT_CDU_L_W,
        EVT_CDU_R_X = CDU_EVT_OFFSET_R + EVT_CDU_L_X,
        EVT_CDU_R_Y = CDU_EVT_OFFSET_R + EVT_CDU_L_Y,
        EVT_CDU_R_Z = CDU_EVT_OFFSET_R + EVT_CDU_L_Z,
        EVT_CDU_R_SPACE = CDU_EVT_OFFSET_R + EVT_CDU_L_SPACE,
        EVT_CDU_R_DEL = CDU_EVT_OFFSET_R + EVT_CDU_L_DEL,
        EVT_CDU_R_SLASH = CDU_EVT_OFFSET_R + EVT_CDU_L_SLASH,
        EVT_CDU_R_CLR = CDU_EVT_OFFSET_R + EVT_CDU_L_CLR,
        EVT_CDU_R_BRITENESS = CDU_EVT_OFFSET_R + EVT_CDU_L_BRITENESS,

        EVT_CDU_C_L1 = THIRD_PARTY_EVENT_ID_MIN + 653,
        CDU_EVT_OFFSET_C = EVT_CDU_C_L1 - EVT_CDU_L_L1,
        EVT_CDU_C_L2 = CDU_EVT_OFFSET_C + EVT_CDU_L_L2,
        EVT_CDU_C_L3 = CDU_EVT_OFFSET_C + EVT_CDU_L_L3,
        EVT_CDU_C_L4 = CDU_EVT_OFFSET_C + EVT_CDU_L_L4,
        EVT_CDU_C_L5 = CDU_EVT_OFFSET_C + EVT_CDU_L_L5,
        EVT_CDU_C_L6 = CDU_EVT_OFFSET_C + EVT_CDU_L_L6,
        EVT_CDU_C_R1 = CDU_EVT_OFFSET_C + EVT_CDU_L_R1,
        EVT_CDU_C_R2 = CDU_EVT_OFFSET_C + EVT_CDU_L_R2,
        EVT_CDU_C_R3 = CDU_EVT_OFFSET_C + EVT_CDU_L_R3,
        EVT_CDU_C_R4 = CDU_EVT_OFFSET_C + EVT_CDU_L_R4,
        EVT_CDU_C_R5 = CDU_EVT_OFFSET_C + EVT_CDU_L_R5,
        EVT_CDU_C_R6 = CDU_EVT_OFFSET_C + EVT_CDU_L_R6,
        EVT_CDU_C_INIT_REF = CDU_EVT_OFFSET_C + EVT_CDU_L_INIT_REF,
        EVT_CDU_C_RTE = CDU_EVT_OFFSET_C + EVT_CDU_L_RTE,
        EVT_CDU_C_DEP_ARR = CDU_EVT_OFFSET_C + EVT_CDU_L_DEP_ARR,
        EVT_CDU_C_ALTN = CDU_EVT_OFFSET_C + EVT_CDU_L_ALTN,
        EVT_CDU_C_VNAV = CDU_EVT_OFFSET_C + EVT_CDU_L_VNAV,
        EVT_CDU_C_FIX = CDU_EVT_OFFSET_C + EVT_CDU_L_FIX,
        EVT_CDU_C_LEGS = CDU_EVT_OFFSET_C + EVT_CDU_L_LEGS,
        EVT_CDU_C_HOLD = CDU_EVT_OFFSET_C + EVT_CDU_L_HOLD,
        EVT_CDU_C_FMCCOMM = THIRD_PARTY_EVENT_ID_MIN + 6721,
        EVT_CDU_C_PROG = CDU_EVT_OFFSET_C + EVT_CDU_L_PROG,
        EVT_CDU_C_EXEC = CDU_EVT_OFFSET_C + EVT_CDU_L_EXEC,
        EVT_CDU_C_MENU = CDU_EVT_OFFSET_C + EVT_CDU_L_MENU,
        EVT_CDU_C_NAV_RAD = CDU_EVT_OFFSET_C + EVT_CDU_L_NAV_RAD,
        EVT_CDU_C_PREV_PAGE = CDU_EVT_OFFSET_C + EVT_CDU_L_PREV_PAGE,
        EVT_CDU_C_NEXT_PAGE = CDU_EVT_OFFSET_C + EVT_CDU_L_NEXT_PAGE,
        EVT_CDU_C_1 = CDU_EVT_OFFSET_C + EVT_CDU_L_1,
        EVT_CDU_C_2 = CDU_EVT_OFFSET_C + EVT_CDU_L_2,
        EVT_CDU_C_3 = CDU_EVT_OFFSET_C + EVT_CDU_L_3,
        EVT_CDU_C_4 = CDU_EVT_OFFSET_C + EVT_CDU_L_4,
        EVT_CDU_C_5 = CDU_EVT_OFFSET_C + EVT_CDU_L_5,
        EVT_CDU_C_6 = CDU_EVT_OFFSET_C + EVT_CDU_L_6,
        EVT_CDU_C_7 = CDU_EVT_OFFSET_C + EVT_CDU_L_7,
        EVT_CDU_C_8 = CDU_EVT_OFFSET_C + EVT_CDU_L_8,
        EVT_CDU_C_9 = CDU_EVT_OFFSET_C + EVT_CDU_L_9,
        EVT_CDU_C_DOT = CDU_EVT_OFFSET_C + EVT_CDU_L_DOT,
        EVT_CDU_C_0 = CDU_EVT_OFFSET_C + EVT_CDU_L_0,
        EVT_CDU_C_PLUS_MINUS = CDU_EVT_OFFSET_C + EVT_CDU_L_PLUS_MINUS,
        EVT_CDU_C_A = CDU_EVT_OFFSET_C + EVT_CDU_L_A,
        EVT_CDU_C_B = CDU_EVT_OFFSET_C + EVT_CDU_L_B,
        EVT_CDU_C_C = CDU_EVT_OFFSET_C + EVT_CDU_L_C,
        EVT_CDU_C_D = CDU_EVT_OFFSET_C + EVT_CDU_L_D,
        EVT_CDU_C_E = CDU_EVT_OFFSET_C + EVT_CDU_L_E,
        EVT_CDU_C_F = CDU_EVT_OFFSET_C + EVT_CDU_L_F,
        EVT_CDU_C_G = CDU_EVT_OFFSET_C + EVT_CDU_L_G,
        EVT_CDU_C_H = CDU_EVT_OFFSET_C + EVT_CDU_L_H,
        EVT_CDU_C_I = CDU_EVT_OFFSET_C + EVT_CDU_L_I,
        EVT_CDU_C_J = CDU_EVT_OFFSET_C + EVT_CDU_L_J,
        EVT_CDU_C_K = CDU_EVT_OFFSET_C + EVT_CDU_L_K,
        EVT_CDU_C_L = CDU_EVT_OFFSET_C + EVT_CDU_L_L,
        EVT_CDU_C_M = CDU_EVT_OFFSET_C + EVT_CDU_L_M,
        EVT_CDU_C_N = CDU_EVT_OFFSET_C + EVT_CDU_L_N,
        EVT_CDU_C_O = CDU_EVT_OFFSET_C + EVT_CDU_L_O,
        EVT_CDU_C_P = CDU_EVT_OFFSET_C + EVT_CDU_L_P,
        EVT_CDU_C_Q = CDU_EVT_OFFSET_C + EVT_CDU_L_Q,
        EVT_CDU_C_R = CDU_EVT_OFFSET_C + EVT_CDU_L_R,
        EVT_CDU_C_S = CDU_EVT_OFFSET_C + EVT_CDU_L_S,
        EVT_CDU_C_T = CDU_EVT_OFFSET_C + EVT_CDU_L_T,
        EVT_CDU_C_U = CDU_EVT_OFFSET_C + EVT_CDU_L_U,
        EVT_CDU_C_V = CDU_EVT_OFFSET_C + EVT_CDU_L_V,
        EVT_CDU_C_W = CDU_EVT_OFFSET_C + EVT_CDU_L_W,
        EVT_CDU_C_X = CDU_EVT_OFFSET_C + EVT_CDU_L_X,
        EVT_CDU_C_Y = CDU_EVT_OFFSET_C + EVT_CDU_L_Y,
        EVT_CDU_C_Z = CDU_EVT_OFFSET_C + EVT_CDU_L_Z,
        EVT_CDU_C_SPACE = CDU_EVT_OFFSET_C + EVT_CDU_L_SPACE,
        EVT_CDU_C_DEL = CDU_EVT_OFFSET_C + EVT_CDU_L_DEL,
        EVT_CDU_C_SLASH = CDU_EVT_OFFSET_C + EVT_CDU_L_SLASH,
        EVT_CDU_C_CLR = CDU_EVT_OFFSET_C + EVT_CDU_L_CLR,
        EVT_CDU_C_BRITENESS = CDU_EVT_OFFSET_C + EVT_CDU_L_BRITENESS,

        // Pedestal - Fwd Aisle Stand
        EVT_PED_DSPL_CTRL_SOURCE_C = THIRD_PARTY_EVENT_ID_MIN + 478,
        EVT_PED_EICAS_EVENT_RCD = THIRD_PARTY_EVENT_ID_MIN + 479,
        EVT_PED_UPPER_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 480,
        EVT_PED_LOWER_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 481,
        EVT_PED_LOWER_TERR_BRIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 4811,
        EVT_PED_L_CCD_SIDE = THIRD_PARTY_EVENT_ID_MIN + 482,
        EVT_PED_L_CCD_INBD = THIRD_PARTY_EVENT_ID_MIN + 483,
        EVT_PED_L_CCD_LWR = THIRD_PARTY_EVENT_ID_MIN + 484,
        EVT_PED_R_CCD_LWR = THIRD_PARTY_EVENT_ID_MIN + 489,
        EVT_PED_R_CCD_INBD = THIRD_PARTY_EVENT_ID_MIN + 490,
        EVT_PED_R_CCD_SIDE = THIRD_PARTY_EVENT_ID_MIN + 491,

        // Pedestal - Control Stand - Fire protection panel
        // Engine 1
        EVT_FIRE_HANDLE_ENGINE_1_TOP = THIRD_PARTY_EVENT_ID_MIN + 651,
        EVT_FIRE_HANDLE_ENGINE_1_BOTTOM = THIRD_PARTY_EVENT_ID_MIN + 6511,
        EVT_FIRE_UNLOCK_SWITCH_ENGINE_1 = THIRD_PARTY_EVENT_ID_MIN + 6512,
        // Engine 2
        EVT_FIRE_HANDLE_ENGINE_2_TOP = THIRD_PARTY_EVENT_ID_MIN + 652,
        EVT_FIRE_HANDLE_ENGINE_2_BOTTOM = THIRD_PARTY_EVENT_ID_MIN + 6521,
        EVT_FIRE_UNLOCK_SWITCH_ENGINE_2 = THIRD_PARTY_EVENT_ID_MIN + 6522,

        // Pedestal - Control Stand - Flaps
        EVT_ALTN_FLAPS_ARM = THIRD_PARTY_EVENT_ID_MIN + 510,
        EVT_ALTN_FLAPS_ARM_GUARD = THIRD_PARTY_EVENT_ID_MIN + 511,
        EVT_ALTN_FLAPS_POS = THIRD_PARTY_EVENT_ID_MIN + 512,

        // Pedestal - Control Stand - Fuel Control
        EVT_CONTROL_STAND_ENG1_START_LEVER = THIRD_PARTY_EVENT_ID_MIN + 520,
        EVT_CONTROL_STAND_ENG2_START_LEVER = THIRD_PARTY_EVENT_ID_MIN + 521,

        // Pedestal - Aft Aisle Stand - COMM Panels
        EVT_COM1_HF_SENSOR_KNOB = THIRD_PARTY_EVENT_ID_MIN + 525,

        //EVT_COM1_START_RANGE = EVT_COM1_HF_SENSOR_KNOB,
        EVT_COM1_TRANSFER_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 526,
        EVT_COM1_OUTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 527,
        EVT_COM1_INNER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 528,
        EVT_COM1_VHFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 529,
        EVT_COM1_VHFC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 530,
        EVT_COM1_VHFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 531,
        EVT_COM1_PNL_OFF_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 532,
        EVT_COM1_HFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 533,
        EVT_COM1_AM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 534,
        EVT_COM1_HFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 535,
        //EVT_COM1_END_RANGE = EVT_COM1_HFR_SWITCH,

        EVT_COM2_HFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 792,

        //EVT_COM2_START_RANGE = EVT_COM2_HFR_SWITCH,
        EVT_COM2_AM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 793,
        EVT_COM2_HFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 794,
        EVT_COM2_PNL_OFF_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 795,
        EVT_COM2_VHFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 796,
        EVT_COM2_VHFC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 797,
        EVT_COM2_VHFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 798,
        EVT_COM2_INNER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 799,
        EVT_COM2_OUTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 800,
        EVT_COM2_TRANSFER_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 801,
        EVT_COM2_HF_SENSOR_KNOB = THIRD_PARTY_EVENT_ID_MIN + 802,
        //EVT_COM2_END_RANGE = EVT_COM2_HF_SENSOR_KNOB,

        EVT_COM3_HF_SENSOR_KNOB = THIRD_PARTY_EVENT_ID_MIN + 596,

        //EVT_COM3_START_RANGE = EVT_COM3_HF_SENSOR_KNOB,
        EVT_COM3_TRANSFER_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 597,
        EVT_COM3_OUTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 599,
        EVT_COM3_INNER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 598,
        EVT_COM3_VHFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 600,
        EVT_COM3_VHFC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 601,
        EVT_COM3_VHFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 602,
        EVT_COM3_PNL_OFF_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 603,
        EVT_COM3_HFL_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 604,
        EVT_COM3_AM_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 605,
        EVT_COM3_HFR_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 606,
        //EVT_COM3_END_RANGE = EVT_COM3_HFR_SWITCH,

        // Pedestal - Aft Aisle Stand - ACP Captain
        EVT_ACP_CAPT_MIC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 536,
        EVT_ACP_CAPT_MIC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 537,
        EVT_ACP_CAPT_MIC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 538,
        EVT_ACP_CAPT_MIC_FLT = THIRD_PARTY_EVENT_ID_MIN + 539,
        EVT_ACP_CAPT_MIC_CAB = THIRD_PARTY_EVENT_ID_MIN + 540,
        EVT_ACP_CAPT_MIC_PA = THIRD_PARTY_EVENT_ID_MIN + 541,
        EVT_ACP_CAPT_MIC_HFL = THIRD_PARTY_EVENT_ID_MIN + 555,
        EVT_ACP_CAPT_MIC_HFR = THIRD_PARTY_EVENT_ID_MIN + 556,
        EVT_ACP_CAPT_MIC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 557,
        EVT_ACP_CAPT_MIC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 558,
        EVT_ACP_CAPT_REC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 548,
        EVT_ACP_CAPT_REC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 549,
        EVT_ACP_CAPT_REC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 550,
        EVT_ACP_CAPT_REC_FLT = THIRD_PARTY_EVENT_ID_MIN + 551,
        EVT_ACP_CAPT_REC_CAB = THIRD_PARTY_EVENT_ID_MIN + 552,
        EVT_ACP_CAPT_REC_PA = THIRD_PARTY_EVENT_ID_MIN + 553,
        EVT_ACP_CAPT_REC_HFL = THIRD_PARTY_EVENT_ID_MIN + 565,
        EVT_ACP_CAPT_REC_HFR = THIRD_PARTY_EVENT_ID_MIN + 566,
        EVT_ACP_CAPT_REC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 567,
        EVT_ACP_CAPT_REC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 568,
        EVT_ACP_CAPT_REC_SPKR = THIRD_PARTY_EVENT_ID_MIN + 564,
        EVT_ACP_CAPT_REC_VORADF = THIRD_PARTY_EVENT_ID_MIN + 571,
        EVT_ACP_CAPT_REC_APP = THIRD_PARTY_EVENT_ID_MIN + 573,
        EVT_ACP_CAPT_MIC_INT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 554,
        EVT_ACP_CAPT_FILTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 572,
        EVT_ACP_CAPT_VOR_ADF_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 569,
        EVT_ACP_CAPT_APP_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 575,
        //EVT_ACP_CAPT_FIRST = EVT_ACP_CAPT_MIC_VHFL,
        //EVT_ACP_CAPT_LAST = EVT_ACP_CAPT_APP_SELECTOR,

        // Pedestal - Aft Aisle Stand - ACP F/O
        EVT_ACP_FO_MIC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 791,
        EVT_ACP_FO_MIC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 790,
        EVT_ACP_FO_MIC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 789,
        EVT_ACP_FO_MIC_FLT = THIRD_PARTY_EVENT_ID_MIN + 788,
        EVT_ACP_FO_MIC_CAB = THIRD_PARTY_EVENT_ID_MIN + 787,
        EVT_ACP_FO_MIC_PA = THIRD_PARTY_EVENT_ID_MIN + 786,
        EVT_ACP_FO_MIC_HFL = THIRD_PARTY_EVENT_ID_MIN + 772,
        EVT_ACP_FO_MIC_HFR = THIRD_PARTY_EVENT_ID_MIN + 771,
        EVT_ACP_FO_MIC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 770,
        EVT_ACP_FO_MIC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 769,
        EVT_ACP_FO_REC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 779,
        EVT_ACP_FO_REC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 778,
        EVT_ACP_FO_REC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 777,
        EVT_ACP_FO_REC_FLT = THIRD_PARTY_EVENT_ID_MIN + 776,
        EVT_ACP_FO_REC_CAB = THIRD_PARTY_EVENT_ID_MIN + 775,
        EVT_ACP_FO_REC_PA = THIRD_PARTY_EVENT_ID_MIN + 774,
        EVT_ACP_FO_REC_HFL = THIRD_PARTY_EVENT_ID_MIN + 762,
        EVT_ACP_FO_REC_HFR = THIRD_PARTY_EVENT_ID_MIN + 761,
        EVT_ACP_FO_REC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 760,
        EVT_ACP_FO_REC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 759,
        EVT_ACP_FO_REC_SPKR = THIRD_PARTY_EVENT_ID_MIN + 763,
        EVT_ACP_FO_REC_VORADF = THIRD_PARTY_EVENT_ID_MIN + 756,
        EVT_ACP_FO_REC_APP = THIRD_PARTY_EVENT_ID_MIN + 754,
        EVT_ACP_FO_MIC_INT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 773,
        EVT_ACP_FO_FILTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 755,
        EVT_ACP_FO_VOR_ADF_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 758,
        EVT_ACP_FO_APP_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 752,
        //EVT_ACP_FO_FIRST = EVT_ACP_FO_APP_SELECTOR,
        //EVT_ACP_FO_LAST = EVT_ACP_FO_MIC_VHFL,

        // Pedestal - Aft Aisle Stand - ACP Observer
        EVT_ACP_OBS_MIC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 607,
        EVT_ACP_OBS_MIC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 608,
        EVT_ACP_OBS_MIC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 609,
        EVT_ACP_OBS_MIC_FLT = THIRD_PARTY_EVENT_ID_MIN + 610,
        EVT_ACP_OBS_MIC_CAB = THIRD_PARTY_EVENT_ID_MIN + 611,
        EVT_ACP_OBS_MIC_PA = THIRD_PARTY_EVENT_ID_MIN + 612,
        EVT_ACP_OBS_MIC_HFL = THIRD_PARTY_EVENT_ID_MIN + 626,
        EVT_ACP_OBS_MIC_HFR = THIRD_PARTY_EVENT_ID_MIN + 627,
        EVT_ACP_OBS_MIC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 628,
        EVT_ACP_OBS_MIC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 629,
        EVT_ACP_OBS_REC_VHFL = THIRD_PARTY_EVENT_ID_MIN + 619,
        EVT_ACP_OBS_REC_VHFC = THIRD_PARTY_EVENT_ID_MIN + 620,
        EVT_ACP_OBS_REC_VHFR = THIRD_PARTY_EVENT_ID_MIN + 621,
        EVT_ACP_OBS_REC_FLT = THIRD_PARTY_EVENT_ID_MIN + 622,
        EVT_ACP_OBS_REC_CAB = THIRD_PARTY_EVENT_ID_MIN + 623,
        EVT_ACP_OBS_REC_PA = THIRD_PARTY_EVENT_ID_MIN + 624,
        EVT_ACP_OBS_REC_HFL = THIRD_PARTY_EVENT_ID_MIN + 636,
        EVT_ACP_OBS_REC_HFR = THIRD_PARTY_EVENT_ID_MIN + 637,
        EVT_ACP_OBS_REC_SAT1 = THIRD_PARTY_EVENT_ID_MIN + 638,
        EVT_ACP_OBS_REC_SAT2 = THIRD_PARTY_EVENT_ID_MIN + 639,
        EVT_ACP_OBS_REC_SPKR = THIRD_PARTY_EVENT_ID_MIN + 635,
        EVT_ACP_OBS_REC_VORADF = THIRD_PARTY_EVENT_ID_MIN + 642,
        EVT_ACP_OBS_REC_APP = THIRD_PARTY_EVENT_ID_MIN + 644,
        EVT_ACP_OBS_MIC_INT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 625,
        EVT_ACP_OBS_FILTER_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 643,
        EVT_ACP_OBS_VOR_ADF_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 640,
        EVT_ACP_OBS_APP_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 646,
        //EVT_ACP_OBS_FIRST = EVT_ACP_OBS_MIC_VHFL,
        //EVT_ACP_OBS_LAST = EVT_ACP_OBS_APP_SELECTOR,

        // Pedestal - Aft Aisle Stand - WX RADAR panel
        EVT_WXR_L_TFR = THIRD_PARTY_EVENT_ID_MIN + 576,
        EVT_WXR_L_WX = THIRD_PARTY_EVENT_ID_MIN + 577,
        EVT_WXR_L_WX_T = THIRD_PARTY_EVENT_ID_MIN + 578,
        EVT_WXR_L_MAP = THIRD_PARTY_EVENT_ID_MIN + 579,
        EVT_WXR_L_GC = THIRD_PARTY_EVENT_ID_MIN + 580,
        EVT_WXR_AUTO = THIRD_PARTY_EVENT_ID_MIN + 583,
        EVT_WXR_L_R = THIRD_PARTY_EVENT_ID_MIN + 584,
        EVT_WXR_TEST = THIRD_PARTY_EVENT_ID_MIN + 585,
        EVT_WXR_R_TFR = THIRD_PARTY_EVENT_ID_MIN + 588,
        EVT_WXR_R_WX = THIRD_PARTY_EVENT_ID_MIN + 589,
        EVT_WXR_R_WX_T = THIRD_PARTY_EVENT_ID_MIN + 590,
        EVT_WXR_R_MAP = THIRD_PARTY_EVENT_ID_MIN + 591,
        EVT_WXR_R_GC = THIRD_PARTY_EVENT_ID_MIN + 592,
        EVT_WXR_L_TILT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 581,
        EVT_WXR_L_GAIN_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 582,
        EVT_WXR_R_TILT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 586,
        EVT_WXR_R_GAIN_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 587,

        // Pedestal - Aft Aisle Stand - TCAS panel
        EVT_TCAS_ALTSOURCE = THIRD_PARTY_EVENT_ID_MIN + 743,
        /* For the new XPDR with ABV/BLW and ABS/REL selectors -----------------------------------*/
        EVT_TCAS_L_ABV_BLW = THIRD_PARTY_EVENT_ID_MIN + 7431,
        EVT_TCAS_L_ABS_REL = THIRD_PARTY_EVENT_ID_MIN + 7432,
        EVT_TCAS_R_ABV_BLW = THIRD_PARTY_EVENT_ID_MIN + 7433,
        EVT_TCAS_R_ABS_REL = THIRD_PARTY_EVENT_ID_MIN + 7434,
        /* -------------------------------------------------------------------------------------- */
        EVT_TCAS_KNOB_L_OUTER = THIRD_PARTY_EVENT_ID_MIN + 744,
        EVT_TCAS_KNOB_L_INNER = THIRD_PARTY_EVENT_ID_MIN + 745,
        EVT_TCAS_IDENT = THIRD_PARTY_EVENT_ID_MIN + 746,
        EVT_TCAS_KNOB_R_OUTER = THIRD_PARTY_EVENT_ID_MIN + 747,
        EVT_TCAS_KNOB_R_INNER = THIRD_PARTY_EVENT_ID_MIN + 748,
        EVT_TCAS_MODE = THIRD_PARTY_EVENT_ID_MIN + 749,
        EVT_TCAS_TEST = THIRD_PARTY_EVENT_ID_MIN + 7491,
        EVT_TCAS_XPNDR = THIRD_PARTY_EVENT_ID_MIN + 751,

        // Pedestal - Aft Aisle Stand - CALL Panel 				(Freighter only)
        EVT_PED_CALL_GND = THIRD_PARTY_EVENT_ID_MIN + 1078,

        //EVT_PED_CALL_PANEL_FIRST_SWITCH = EVT_PED_CALL_GND,
        EVT_PED_CALL_CREW_REST = THIRD_PARTY_EVENT_ID_MIN + 1079,
        EVT_PED_CALL_SUPRNMRY = THIRD_PARTY_EVENT_ID_MIN + 1080,
        EVT_PED_CALL_CARGO = THIRD_PARTY_EVENT_ID_MIN + 1081,
        EVT_PED_CALL_CARGO_AUDIO = THIRD_PARTY_EVENT_ID_MIN + 1082,
        EVT_PED_CALL_MAIN_DK_ALERT = THIRD_PARTY_EVENT_ID_MIN + 1083,
        //EVT_PED_CALL_PANEL_LAST_SWITCH = EVT_PED_CALL_MAIN_DK_ALERT,

        // Pedestal - Aft Aisle Stand - Various
        EVT_PED_OBS_AUDIO_SELECTOR = THIRD_PARTY_EVENT_ID_MIN + 648,
        EVT_FCTL_AILERON_TRIM = THIRD_PARTY_EVENT_ID_MIN + 727,
        EVT_FCTL_RUDDER_TRIM = THIRD_PARTY_EVENT_ID_MIN + 728,
        EVT_FCTL_RUDDER_TRIM_CANCEL = THIRD_PARTY_EVENT_ID_MIN + 729,
        EVT_PED_FLOOR_LIGHTS = THIRD_PARTY_EVENT_ID_MIN + 735,
        EVT_PED_PANEL_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 736,
        EVT_PED_FLOOD_LIGHT_CONTROL = THIRD_PARTY_EVENT_ID_MIN + 737,
        EVT_PED_EVAC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 739,
        EVT_PED_EVAC_SWITCH_GUARD = THIRD_PARTY_EVENT_ID_MIN + 740,
        EVT_PED_EVAC_HORN_SHUTOFF = THIRD_PARTY_EVENT_ID_MIN + 741,
        EVT_PED_EVAC_TEST_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 742,

        // Pedestal - Control Stand
        //
        EVT_CONTROL_STAND_SPEED_BRAKE_LEVER = THIRD_PARTY_EVENT_ID_MIN + 498,
        EVT_CONTROL_STAND_SPEED_BRAKE_LEVER_DOWN = THIRD_PARTY_EVENT_ID_MIN + 4981,
        EVT_CONTROL_STAND_SPEED_BRAKE_LEVER_ARM = THIRD_PARTY_EVENT_ID_MIN + 4982,
        EVT_CONTROL_STAND_SPEED_BRAKE_LEVER_UP = THIRD_PARTY_EVENT_ID_MIN + 4983,
        EVT_CONTROL_STAND_SPEED_BRAKE_LEVER_50 = THIRD_PARTY_EVENT_ID_MIN + 4984,

        EVT_CONTROL_STAND_FLAPS_LEVER = THIRD_PARTY_EVENT_ID_MIN + 507,
        EVT_CONTROL_STAND_FLAPS_LEVER_0 = THIRD_PARTY_EVENT_ID_MIN + 5071,
        EVT_CONTROL_STAND_FLAPS_LEVER_1 = THIRD_PARTY_EVENT_ID_MIN + 5072,
        EVT_CONTROL_STAND_FLAPS_LEVER_5 = THIRD_PARTY_EVENT_ID_MIN + 5073,
        EVT_CONTROL_STAND_FLAPS_LEVER_15 = THIRD_PARTY_EVENT_ID_MIN + 5074,
        EVT_CONTROL_STAND_FLAPS_LEVER_20 = THIRD_PARTY_EVENT_ID_MIN + 5075,
        EVT_CONTROL_STAND_FLAPS_LEVER_25 = THIRD_PARTY_EVENT_ID_MIN + 5076,
        EVT_CONTROL_STAND_FLAPS_LEVER_30 = THIRD_PARTY_EVENT_ID_MIN + 5077,

        EVT_CONTROL_STAND_ALT_PITCH_TRIM_LEVER = THIRD_PARTY_EVENT_ID_MIN + 496,
        EVT_CONTROL_STAND_REV_THRUST1_LEVER = THIRD_PARTY_EVENT_ID_MIN + 499,
        EVT_CONTROL_STAND_REV_THRUST2_LEVER = THIRD_PARTY_EVENT_ID_MIN + 503,
        EVT_CONTROL_STAND_FWD_THRUST1_LEVER = THIRD_PARTY_EVENT_ID_MIN + 501,
        EVT_CONTROL_STAND_FWD_THRUST2_LEVER = THIRD_PARTY_EVENT_ID_MIN + 505,
        EVT_CONTROL_STAND_AT1_DISENGAGE_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 502,
        EVT_CONTROL_STAND_AT2_DISENGAGE_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 506,
        EVT_CONTROL_STAND_TOGA1_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 500,
        EVT_CONTROL_STAND_TOGA2_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 504,
        EVT_CONTROL_STAND_PARK_BRAKE_LEVER = THIRD_PARTY_EVENT_ID_MIN + 515,
        EVT_CONTROL_STAND_STABCUTOUT_SWITCH_C_GUARD = THIRD_PARTY_EVENT_ID_MIN + 516,
        EVT_CONTROL_STAND_STABCUTOUT_SWITCH_C = THIRD_PARTY_EVENT_ID_MIN + 517,
        EVT_CONTROL_STAND_STABCUTOUT_SWITCH_R_GUARD = THIRD_PARTY_EVENT_ID_MIN + 518,
        EVT_CONTROL_STAND_STABCUTOUT_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 519,

        // Oxygen Panels
        //
        EVT_OXY_TEST_RESET_SWITCH_L = THIRD_PARTY_EVENT_ID_MIN + 1063,
        EVT_OXY_TEST_RESET_SWITCH_R = THIRD_PARTY_EVENT_ID_MIN + 1066,
        EVT_OXY_TEST_RESET_SWITCH_OBS_R = THIRD_PARTY_EVENT_ID_MIN + 1069,
        EVT_OXY_TEST_RESET_SWITCH_OBS_L = THIRD_PARTY_EVENT_ID_MIN + 1072,
        EVT_OXY_NORM_EMER_L = THIRD_PARTY_EVENT_ID_MIN + 1064,
        EVT_OXY_NORM_EMER_R = THIRD_PARTY_EVENT_ID_MIN + 1067,
        EVT_OXY_NORM_EMER_OBS_R = THIRD_PARTY_EVENT_ID_MIN + 1070,
        EVT_OXY_NORM_EMER_OBS_L = THIRD_PARTY_EVENT_ID_MIN + 1073,

        // miscellaneous
        //
        EVT_YOKE_AP_DISC_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1084,
        EVT_CLICKSPOT_ISFD = THIRD_PARTY_EVENT_ID_MIN + 1101,
        EVT_CLICKSPOT_STBY_ADI = THIRD_PARTY_EVENT_ID_MIN + 1102,
        EVT_CLICKSPOT_STBY_ASI = THIRD_PARTY_EVENT_ID_MIN + 1103,
        EVT_CLICKSPOT_STBY_ALTIMETER = THIRD_PARTY_EVENT_ID_MIN + 1104,
        EVT_CLICKSPOT_GMCS_ZOOM = THIRD_PARTY_EVENT_ID_MIN + 1112,
        
        // CA and FO Armrests
        EVT_CA_ARMREST_LEFT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1006,
        EVT_CA_ARMREST_RIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1007,
        EVT_FO_ARMREST_LEFT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1008,
        EVT_FO_ARMREST_RIGHT_SWITCH = THIRD_PARTY_EVENT_ID_MIN + 1009,
        
        //
        // Custom shortcut special events
        //
        EVT_LDG_LIGHTS_TOGGLE = THIRD_PARTY_EVENT_ID_MIN + 14000,
        EVT_TURNOFF_LIGHTS_TOGGLE = THIRD_PARTY_EVENT_ID_MIN + 14001,
        EVT_COCKPIT_LIGHTS_TOGGLE = THIRD_PARTY_EVENT_ID_MIN + 14002,
        EVT_PANEL_LIGHTS_TOGGLE = THIRD_PARTY_EVENT_ID_MIN + 14003,
        EVT_FLOOD_LIGHTS_TOGGLE = THIRD_PARTY_EVENT_ID_MIN + 14004,

        EVT_DOOR_1L = THIRD_PARTY_EVENT_ID_MIN + 14011,
        EVT_DOOR_1R = THIRD_PARTY_EVENT_ID_MIN + 14012,
        EVT_DOOR_2L = THIRD_PARTY_EVENT_ID_MIN + 14013,
        EVT_DOOR_2R = THIRD_PARTY_EVENT_ID_MIN + 14014,
        EVT_DOOR_3L = THIRD_PARTY_EVENT_ID_MIN + 14015,
        EVT_DOOR_3R = THIRD_PARTY_EVENT_ID_MIN + 14016,
        EVT_DOOR_4L = THIRD_PARTY_EVENT_ID_MIN + 14017,
        EVT_DOOR_4R = THIRD_PARTY_EVENT_ID_MIN + 14018,
        EVT_DOOR_5L = THIRD_PARTY_EVENT_ID_MIN + 14019,
        EVT_DOOR_5R = THIRD_PARTY_EVENT_ID_MIN + 14020,
        EVT_DOOR_CARGO_FWD = THIRD_PARTY_EVENT_ID_MIN + 14021,
        EVT_DOOR_CARGO_AFT = THIRD_PARTY_EVENT_ID_MIN + 14022,
        EVT_DOOR_CARGO_BULK = THIRD_PARTY_EVENT_ID_MIN + 14023,
        EVT_DOOR_CARGO_MAIN = THIRD_PARTY_EVENT_ID_MIN + 14024,
        EVT_DOOR_FWD_ACCESS = THIRD_PARTY_EVENT_ID_MIN + 14025,
        EVT_DOOR_EE_ACCESS = THIRD_PARTY_EVENT_ID_MIN + 14026,
        EVT_AT_ARM_SWITCHES = THIRD_PARTY_EVENT_ID_MIN + 14027,
        
        //
        // Window handle, crank and clipboard events
        //
        EVT_DOOR_WINDOW_CA = THIRD_PARTY_EVENT_ID_MIN + 1012,
        EVT_DOOR_WINDOW_FO = THIRD_PARTY_EVENT_ID_MIN + 1013,
        EVT_WINDOW_CA_HANDLE = THIRD_PARTY_EVENT_ID_MIN + 1014,
        EVT_WINDOW_CA_CLIPBOARD = THIRD_PARTY_EVENT_ID_MIN + 1015,
        EVT_WINDOW_FO_HANDLE = THIRD_PARTY_EVENT_ID_MIN + 1016,
        EVT_WINDOW_FO_CLIPBOARD = THIRD_PARTY_EVENT_ID_MIN + 1017,

        // MCP Direct Control 
        EVT_MCP_IAS_SET = THIRD_PARTY_EVENT_ID_MIN + 14502,    // Sets MCP IAS, if IAS mode is active
        EVT_MCP_MACH_SET = THIRD_PARTY_EVENT_ID_MIN + 14503,   // Sets MCP MACH (if active) to parameter*0.001 (e.g. send 780 to set M0.780)
        EVT_MCP_HDGTRK_SET = THIRD_PARTY_EVENT_ID_MIN + 14504, // Sets new heading  or track, commands the shortest turn
        EVT_MCP_ALT_SET = THIRD_PARTY_EVENT_ID_MIN + 14505,
        EVT_MCP_VS_SET = THIRD_PARTY_EVENT_ID_MIN + 14506,  // Sets MCP VS (if VS window open) to parameter-10000 (e.g. send 8200 for -1800fpm)
        EVT_MCP_FPA_SET = THIRD_PARTY_EVENT_ID_MIN + 14507, // Sets MCP FPA (if VS window open) to (parameter*0.1-10) (e.g. send 82 for -1.8 degrees)

        // Panel system events
        //EVT_CTRL_ACCELERATION_DISABLE = THIRD_PARTY_EVENT_ID_MIN + 14600,
        //EVT_CTRL_ACCELERATION_ENABLE = THIRD_PARTY_EVENT_ID_MIN + 14600,
        //EVT_2D_PANEL_OFFSET = 20000,  // added to events trigger by 2D panel pop-up windows
    }
}