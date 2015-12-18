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
float DownPistonResetSpeed = -1.0f; // downwards piston reset speed
float SidePistonResetSpeed = -1.0f; // sideways piston reset speed
float RotorResetSpeed = -1.5f; // rotor reset speed

/* Don't change anything below these lines!
*/

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
   for(var i=0; i < blocks.Count; i++)
   {
      var drill = (IMyShipDrill)blocks[i];
      Drills.Add(drill);
      if (UseLCDDebug) { Debug("- Drill: " + drill.Name.ToString() + " found", true); }
   }

   // populate downwards piston list
   var group = GridTerminalSystem.BlockGroups.Find(delegate(IMyBlockGroup blockGroup) { return blockGroup.Name == DownPistonGroupName; });
   var blocks = group.Blocks;
   for(var i=0; i < blocks.Count; i++)
   {
      var piston = (IMyPistonBase)blocks[i];
      DownPistons.Add(piston);
      if (UseLCDDebug) { Debug("- DPiston: " + piston.Name.ToString() + " found", true); }
   }

   // populate sidewards piston list if used in this ADS
   if (UseSidePistons)
   {
      var group = GridTerminalSystem.BlockGroups.Find(delegate(IMyBlockGroup blockGroup) { return blockGroup.Name == SidePistonGroupName; });
      var blocks = group.Blocks;
      for(var i=0; i < blocks.Count; i++)
      {
         var piston = (IMyPistonBase)blocks[i];
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
   if (UseLCDStatus) { Status("Initializing ADS system and components ...", false); }

   // setup all drills to init state
   if (UseLCDStatus) { Status("- changing drills to init state", true); }
   if (UseLCDDebug) { Debug("Adjusting drills to init state:", true); }
   for (int i = 0; i < DownPistons.Count; i++)
   {
      IMyShipDrill D = Drills[i];
      D.ApplyAction("OnOff_Off");
      if (UseLCDDebug) { Debug("- " + D.Name.ToString() + " -> offline", true); }
   }

   // setup all pistons to init state
   if (UseLCDStatus) { Status("- Moving downwards pistons to init state", true); }
   if (UseLCDDebug) { Debug("Moving downward pistons to init state:", true); }
   for (int i = 0; i < DownPistons.Count; i++)
   {
      IMyPistonBase P = DownPistons[i];
      P.SetValue("Velocity",DownPistonResetSpeed);
      P.ApplyAction("OnOff_On");
      if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> moving", true); }
   }
   if (UseSidePistons)
   {
      if (UseLCDStatus) { Status("- Moving sideways pistons to init state", true); }
      if (UseLCDDebug) { Debug("Moving sideways pistons to init state:", true); }
      for (int i = 0; i < SidePistons.Count; i++)
      {
         IMyPistonBase P = SidePistons[i];
         P.SetValue("Velocity",SidePistonResetSpeed);
         P.ApplyAction("OnOff_On");
         if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> moving", true); }
      }
   }

   // setup rotor to init state
   if (UseLCDStatus) { Status("- Setting rotor to init state", true); }
   if (UseLCDDebug) { Debug("Setting up rotor init state:", true); }
   ADS_Rotor.SetValue<float>("LowerLimit", RotorLL);
   ADS_Rotor.SetValue<float>("UpperLimit", RotorUL);
   int Rotor_Angle = Convert.ToInt32((ADS_Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));
   if (Rotor_Angle >= RotorLL) and (Rotor_Angle <= (RotorLL + 3)))
   {
      ADS_Rotor.ApplyAction("OnOff_On");
      ADS_Rotor.SetValue("Velocity", RotorResetSpeed);
      if (UseLCDStatus) { Status("- Moving rotor to init state", true); }
      if (UseLCDDebug) { Debug("- " + ADS_Rotor.Name.ToString() + " -> moving", true); }
   } else
   {
      if (UseLCDDebug) { Debug("- " + ADS_Rotor.Name.ToString() + " -> done", true); }
   }  

   // check if all blocks are in init state and prepare normal block settings
   if (UseLCDStatus) { Status("- veryfing init state positions", true); }
   var init_done = true;
   
   // verify DownPiston positions
   if (UseLCDDebug) { Debug("Checking if downwards pistons reached init state:", true); }
   for (int i = 0; i < DownPistons.Count; i++)
   {
      IMyPistonBase P = DownPistons[i];
      double P_Pos = Convert.ToDouble((P.DetailedInfo.Remove(0,28)).TrimEnd('m'));
      if (P_Pos == 0.0f)
      {
         if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> finished", true); }
         P.SetValue("Velocity",0.0f);
         P.ApplyAction("OnOff_Off");
      } else
      {
         init_done = false;
         if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> still moving", true); }
      }
   }
   
   // verify SidePiston positions
   if (UseLCDDebug) { Debug("Checking if sideways pistons reached init state:", true); }
   if (UseSidePistons)
   {
      for (int i = 0; i < SidePistons.Count; i++)
      {
         IMyPistonBase P = SidePistons[i];
         double P_Pos = Convert.ToDouble((P.DetailedInfo.Remove(0,28)).TrimEnd('m'));
         if (P_Pos == 0.0f)
         {
            if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> finished", true); }
            P.SetValue("Velocity",0.0f);
            P.ApplyAction("OnOff_Off");
         } else
         {
            init_done = false;
            if (UseLCDDebug) { Debug("- " + P.Name.ToString() + " -> still moving", true); }
         }
      }
   }
   
   // verify rotor position
   if (UseLCDDebug) { Debug("Checking if rotor reached init state:", true); }
   int Rotor_Angle = Convert.ToInt32((ADS_Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));
   if (Rotor_Angle >= RotorLL) and (Rotor_Angle <= (RotorLL + 3)))
   {
      if (UseLCDDebug) { Debug("- " + ADS_Rotor.Name.ToString() + " -> finished", true); }
      ADS_Rotor.SetValue("Velocity",0.0f);
      ADS_Rotor.ApplyAction("OnOff_Off");
   } else
   {
      init_done = false;
      if (UseLCDDebug) { Debug("- " + ADS_Rotor.Name.ToString() + " -> still moving", true); }
   }

   // change program state to StateMineRing
   if (init_done)
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
