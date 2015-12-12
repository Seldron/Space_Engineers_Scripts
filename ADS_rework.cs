// Automatic Drilling System

// Options
bool UseBeacon = true;
bool CheckStorage = true;
bool UseSidePistons = true;
bool UseLCDStatus = true;
bool UseLCDDebug = true;

// block or blockgroup names
string SidePistonGroupName = "ADS_Pistons_Side";
string DownPistonGroupName = "ADS_Pistons_Down";
string DrillGroupName = "ADS_Drills";
string RotorName = "ADS_Rotor";
string StorageName = "ADS_Storage";
string BeaconName = "ADS_Beacon";
string TimerName = "ADS_Timer";
string CPUName = "ADS_CPU";
string LCDStatusName = "ADS_Status";
string LCDDebugName = "ADS_Debug";

//calculation vars
float DownwardIncrement = 0.5f; // downwards piston extend
float SidewaysIncrement = 2.5f; // sideways pisten extend
float RotorRPM = 0.5f; // Rotor rounds per minute
float RotorLL = 0.0f; // Rotor lower limit default
float RotorUL = 360.0f; // Rotor upper limit default
float DownwardsResetSpeed = 1.0f; // downwards piston reset speed


// program states
int StateStart = 0;
int StateInit = 1;
int StatePrepareRing = 2;
int StateMineRing = 3;
int StatePrepareDisk = 4;
int StateFinished = 5;
int StatePaused = 6;

// Stored Data
float target_pos_side;
float target_pos_down;
float target_angle = 360.0f;
float last_angle;
int last_state;
string current_beacon_name = "ADS_Bracon";
string pause_cause = "";

// define bootup state
int state = 0;

//create lists to gather the sideways pistons, downwards pistons and drills
List<IMyPistonBase> DownPistons = new List<IMyPistonBase>();
List<IMyPistonBase> Drills = new List<IMyPistonBase>();
if (UseSidePistons) { List<IMyPistonBase> SidePistons = new List<IMyPistonBase>(); }

// define ADS blocks
IMyMotorStator ADS_Rotor;
IMyTimerBlock ADS_Timer;
IMyProgrammableBlock ADS_CPU;
if (CheckStorage) { IMyCargoContainer ADS_Storage; }
if (UseBeacon) { IMyBeacon ADS_Beacon; }
if (UseLCDStatus) { IMyTextPanel ADS_Status; }
if (UseLCDDebug) { IMyTextPanel ADS_Debug; }


void Main()
{
   if (state == StateStart)
      start();
   else if (state == StateInit)
      init();
   else if (state == StatePrepareRing)
      preparering();
   else if (state == StateMineRing)
      minering();
   else if (state == StatePrepareDisk)
      preparedisk();
   else if (state == StateFinished)
      finished();
   else if (state == StatePaused)
      paused(pause_cause);
}


void start()
{
   // find ADS_Debug if using Debug LCD is on
   if (UseLCDDebug)
   {
      IMyTextPanel ADS_Debug = GridTerminalSystem.GetBlockWithName(LCDDebugName) as IMyTextPanel;
   }
   if (UseLCDDebug) { Debug("State: start", false); }
   if (UseLCDDebug) { Debug("- Debug screen found", true); }

   // populate drills list
   var group = GridTerminalSystem.BlockGroups.Find(delegate(IMyBlockGroup blockGroup) { return blockGroup.Name == DrillGroupName; });
   var blocks = group.Blocks;
   for(var j=0; j < blocks.Count; j++)
   {
      var drill = (IMyShipDrill)blocks[j];
      Drills.Add(drill);
      if (UseLCDDebug) { Debug("- Drill: " + drill.Name.ToString() + " found", true); }
   }

   // populate downwards piston list
   var group = GridTerminalSystem.BlockGroups.Find(delegate(IMyBlockGroup blockGroup) { return blockGroup.Name == DownPistonGroupName; });
   var blocks = group.Blocks;
   for(var j=0; j < blocks.Count; j++)
   {
      var piston = (IMyPistonBase)blocks[j];
      DownPistons.Add(piston);
      if (UseLCDDebug) { Debug("- DPiston: " + piston.Name.ToString() + " found", true); }
   }

   // populate sidewards piston list if used in this ADS
   if (UseSidePistons)
   {
      var group = GridTerminalSystem.BlockGroups.Find(delegate(IMyBlockGroup blockGroup) { return blockGroup.Name == SidePistonGroupName; });
      var blocks = group.Blocks;
      for(var j=0; j < blocks.Count; j++)
      {
         var piston = (IMyPistonBase)blocks[j];
         SidePistons.Add(piston);
         if (UseLCDDebug) { Debug("- SPiston: " + piston.Name.ToString() + " found", true); }
      }
   }

   // find ADS_Rotor
   IMyMotorStator ADS_Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
   if (UseLCDDebug) { Debug("- Rotor: " + ADS_Rotor.Name.ToString() + " found", true); }

   // find ADS_Timer
   IMyTimerBlock ADS_Timer = GridTerminalSystem.GetBlockWithName(TimerName) as IMyTimerBlock;
   if (UseLCDDebug) { Debug("- Timer: " + ADS_Timer.Name.ToString() + " found", true); }

   // find ADS_CPU
   IMyProgrammableBlock ADS_CPU = GridTerminalSystem.GetBlockWithName(CPUName) as IMyProgrammableBlock;
   if (UseLCDDebug) { Debug("- CPU: " + ADS_CPU.Name.ToString() + " found", true); }

   // find ADS_Storage if checking storage is on
   if (CheckStorage)
   {
      IMyCargoContainer ADS_Storage = GridTerminalSystem.GetBlockWithName(StorageName) as IMyCargoContainer;
      if (UseLCDDebug) { Debug("- Storage: " + ADS_Storage.Name.ToString() + " found", true); }
   }

   // find ADS_Beacon if using beacon is on
   if (UseBeacon)
   {
      IMyBeacon ADS_Beacon = GridTerminalSystem.GetBlockWithName(BeaconName) as IMyBeacon;
      if (UseLCDDebug) { Debug("- Beacon: " + ADS_Beacon.Name.ToString() + " found", true); }
   }

   // find ADS_Status if using Status LCD is on
   if (UseLCDStatus)
   {
      IMyTextPanel ADS_Status = GridTerminalSystem.GetBlockWithName(LCDStatusName) as IMyTextPanel;
      if (UseLCDDebug) { Debug("- StatusLCD: " + ADS_Status.Name.ToString() + " found", true); }
   }

   // change program state to init
   state = StateInit;
   if (UseLCDDebug) { Debug("- ADS State switched to StateInit", true); }

}


void init()
{
   if (UseLCDDebug) { Debug("State: init", false); }

   // init status LCD
   if (UseLCDDebug) { Status("Initializing ADS system and components ...", false); }

   // TODO: move all pistons to angle: 0
   // TODO: move rotor to angle: 0
   // TODO: after movement we set up normal starting states below

   // setup rotor to init state
   ADS_Rotor.ApplyAction("OnOff_Off");
   ADS_Rotor.SetValue<float>("LowerLimit", RotorLL);
   ADS_Rotor.SetValue<float>("UpperLimit", RotorUL);
   ADS_Rotor.SetValue("Velocity", RotorRPM);
   
   // setup all pistons to init state
   
   
   
   // only change state to next state if init mode is finished
   // all pistons pulled in, switched off, 0.1m velocity?
   // rotor switched of, in 0 angle, rotor rpm ok?

   if ()
   {
      state = StateMineRing;
      if (UseLCDDebug) { Debug("- ADS State switched to StateMineRing", true); }
   }
}


void preparering()
{
   // TODO
}


void minering()
{
   // TODO
}

void preparedisk()
{
   // TODO
}


void finished()
{
   // TODO
}


void paused()
{
   // TODO
}


/*
Helper Functions
===================
*/

// move pistons to 0 extension
void ResetAllPistons()
{
   for (int i = 0; i < DownPistons.Count; i++)
   {
      // TODO! CURRENT
      IMyPistonBase P = DownPistons[i];
      P.SetValue("Velocity",-0.1f);
      P.ApplyAction("OnOff_On");
   }
   if (UseSidePistons)
   {
      for (int i = 0; i < SidePistons.Count; i++)
      {
         // TODO! CURRENT
         IMyPistonBase P = DownPistons[i];
         P.SetValue("Velocity",-0.1f);
         P.ApplyAction("OnOff_On");
      }
   }
}

// calculate percent
public double GetPercent(double current, double max) 
{ 
   return (max > 0 ? (current / max) * 100 : 100); 
}

// print to debug LCD
void Debug(string text, bool append) 
{ 
   ADS_Status.WritePublicText(text + "\n" , append); 
} 

// print to status LCD
void Status(string text, bool append) 
{ 
   ADS_Debug.WritePublicText(text + "\n" , append); 
}
