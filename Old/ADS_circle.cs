/* ADS - Automatic Drilling Station:
Make sure to setup the rotor that is uses a velocity which is positive f.e. 0.1 instead of -0.1
Use correct block names or adjust the script to your desired block names 
Name the rotor: ADS_Rotor 
Name the lower arm pistons: ADS_Piston_Down_[1,2] 
Name of the Debug LCD Screen: ADS_Debug 
Name of the Status LCD Screen: ADS_Status
Name of the Timer: ADS_Timer 
Name the Ore Storage Container: ADS_Storage 
Name of the ADS Programmable Block: ADS_CPU
You should try to automatically pull out as much as you can out of this 
 storage container, if its full the ADS will pause working 
Setup the ADS_Debug LCD to show public text.
Setup the ADS_Status LCD to show public text. 
Setup the timer to run the ADS programmable block as its 1. Action.
Run the programmable block named ADS_CPU.
*/ 
 
// stored STATE vars
double ADS_STATE_Storage_Usage;  // Stores the Usage % of ADS_Storage block volume
int ADS_STATE_Rotor_Angle = 0; // Store last measured rotor angle to find out if we have yet made a full circle
double ADS_STATE_PD1_Pos = 0.0f; // Store last measured Piston Down Position to find out if we have yet extended by the value we wanted
double ADS_STATE_PD2_Pos = 0.0f; // Store last measured Piston Down Position to find out if we have yet extended by the value we wanted

int ADS_STATE_PAUSED_Storage = 0; // stores if the ADS is currently paused because of storage problems
int ADS_STATE_PAUSED_FinishedLayer = 0; // stores if the ADS is currently paused because of a finished layer
int ADS_STATE_PAUSED_FinishedAllLayers = 0; // stores if the ADS has finished all possible layers with all pistons extended
int ADS_STATE_Drillhead_Moving_Down = 0; // stores if the ADS is currently moving the drills down
 
void Main() 
{ 
  // clear Debug and Status LCD and show header
  Debug("ADS_DEBUG_Screen:", false);
  PrintStatus("ADS_Status_Screen:", false);

  // ADS_Rotor vars 
  IMyMotorStator ADS_Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator; 
  var ADS_Rotor_UpperLimit = ADS_Rotor.GetValue<float>("UpperLimit"); 
  var ADS_Rotor_LowerLimit = ADS_Rotor.GetValue<float>("LowerLimit"); 
  float ADS_Rotor_Velocity = ADS_Rotor.GetValue<float>("Velocity"); 
//  Debug((ADS_Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'), true);
  int ADS_Rotor_Angle = Convert.ToInt32((ADS_Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));


  // ADS_Piston_Down_1 vars
  IMyPistonBase ADS_PD1 = GridTerminalSystem.GetBlockWithName("ADS_Piston_Down_1") as IMyPistonBase;
//  Debug((ADS_PD1.DetailedInfo.Remove(0,28)).TrimEnd('m'), true);
  double ADS_PD1_Pos = Convert.ToDouble((ADS_PD1.DetailedInfo.Remove(0,28)).TrimEnd('m'));

    // ADS_Piston_Down_2 vars
  IMyPistonBase ADS_PD2 = GridTerminalSystem.GetBlockWithName("ADS_Piston_Down_2") as IMyPistonBase;
//  Debug((ADS_PD2.DetailedInfo.Remove(0,28)).TrimEnd('m'), true);
  double ADS_PD2_Pos = Convert.ToDouble((ADS_PD2.DetailedInfo.Remove(0,28)).TrimEnd('m'));
 
  // ADS Timer vars 
  IMyTimerBlock ADS_Timer = GridTerminalSystem.GetBlockWithName("ADS_Timer")as IMyTimerBlock;  

  // ADS Drill Movement Logic
  // finished mining?
  if (ADS_STATE_PAUSED_FinishedAllLayers == 1) {
    // we are done!
	Pause("FinishedAllLayers");
  } else {
  
  // check if inventory space in ADS_Storage is max 50%, if not stop any rotor movement 
  Debug("checking storage: " + ADS_STATE_Storage_Usage.ToString() + "<= 50 ?", true);  
  VerifyStorage();
  if (ADS_STATE_Storage_Usage <= 50) { 
    Debug("->true", true);
    // if we have been paused resume and reset values
    if (ADS_STATE_PAUSED_Storage == 1) {
      Resume("Storage");
    }
    // check if rotor has made full circle yet (if so we need to pause and move down!)
	Debug("Rotor circle finished?: " + ADS_Rotor_Angle.ToString() + " <= " + ADS_STATE_Rotor_Angle.ToString() + "?", true); 
    if (ADS_Rotor_Angle <= ADS_STATE_Rotor_Angle) {
      Debug("-->true", true); 
      Debug("Drillhead currently in down motion?: " + ADS_STATE_Drillhead_Moving_Down.ToString() + "== 0?", true); 
      // we just finished a full circle
	  // we need to pause rotation because of finished layer
      Pause("FinishedLayer");
	  if (ADS_STATE_Drillhead_Moving_Down == 0) { 
        Debug("--->true", true); 
		// find the piston we can still extend
		// try piston 1:
		if (ADS_PD1_Pos <= 9.5f) {
		  PrintStatus("Starting ADS_Piston_Down_1 ...", true);
		  // save old piston extension value for verification when we finished moving downwards
		  ADS_STATE_PD1_Pos = ADS_PD1_Pos;
		  // set piston speed to 0.1m/s
		  ADS_PD1.SetValue("Velocity",0.1f);
		  // toggle Piston 1 on
		  ADS_PD1.ApplyAction("OnOff_On");
		  // Save to ADS_State that we are moving piston D 1 
		  ADS_STATE_Drillhead_Moving_Down = 1;
		} else {
		  if (ADS_PD2_Pos <= 9.5f) {
		    PrintStatus("Starting ADS_Piston_Down_2 ...", true);
		    // save old piston extension value for verification when we finished moving downwards
		    ADS_STATE_PD2_Pos = ADS_PD1_Pos;
		    // set piston speed to 0.1m/s
		    ADS_PD2.SetValue("Velocity",0.1f);
		    // toggle Piston 2 on
		    ADS_PD2.ApplyAction("OnOff_On");
			// Save to ADS_State that we are moving piston D 2
		    ADS_STATE_Drillhead_Moving_Down = 2;
		  } else {
            Pause("FinishedAllLayers");
		  }
		}
      } else { // Borehead is currently moving Down - verify if finished
        Debug("--->false", true);
        // we are currently moving the borehead down and need to find out if we have reached 
        // enough downward lenght to start rotor again 
		if (ADS_STATE_Drillhead_Moving_Down == 1) {
		  Debug("Finished PistonDown? " + ADS_PD1_Pos.ToString() + " >= 0.5 + " + ADS_STATE_PD1_Pos.ToString(), true);
		  if (ADS_PD1_Pos >= (ADS_STATE_PD1_Pos + 0.5f)) {
		    // finished down movement
			// toggle Piston 1 off
		    ADS_PD1.ApplyAction("OnOff_Off");
			// Resume Rotation
			Resume("FinishedLayer");
			// Save to ADS_State that we are finished moving P Ds 
		    ADS_STATE_Drillhead_Moving_Down = 0;
		  } else { // still to go
			Debug("No, still have to wait for piston downwards movement...", true);
		  }
		} else {
		  if (ADS_STATE_Drillhead_Moving_Down == 2) {
		    Debug("Finished PistonDown? " + ADS_PD2_Pos.ToString() + " >= 0.5 + " + ADS_STATE_PD2_Pos.ToString(), true);
		    if (ADS_PD2_Pos >= (ADS_STATE_PD2_Pos + 0.5f)) {
		      // finished down movement
			  // toggle Piston 2 off
		      ADS_PD2.ApplyAction("OnOff_Off");
			  // Resume Rotation
			  Resume("FinishedLayer");
			  // Save to ADS_State that we are finished moving P Ds 
		      ADS_STATE_Drillhead_Moving_Down = 0;
		    } else { // still to go
			  Debug("No, still have to wait for piston downwards movement...", true);
		    }
		  }
		}
      }
    }
	ADS_STATE_Rotor_Angle = ADS_Rotor_Angle; 
  } else { 
    Debug("->false", true); 
    // Pause operations because storage has not enough space
    Pause("Storage");
  } // storage check finished
  
  } // from layers finished check
  
  // restart timer
// disabled since script is running so fast that the drils dont actually move fast enough to recognise state changes
//  ADS_Timer.GetActionWithName("TriggerNow").Apply(ADS_Timer); 
}


void VerifyStorage()
{
  // fetch ADS_Storage Container Block Object
  IMyCargoContainer ADS_Storage = GridTerminalSystem.GetBlockWithName("ADS_Storage") as IMyCargoContainer;
  // read inventory usage and max usage of the block
  IMyInventory ADS_Storage_Inv = ADS_Storage.GetInventory(0);
  double currentVolume = (double)ADS_Storage_Inv.CurrentVolume; 
  double maxVolume = (double)ADS_Storage_Inv.MaxVolume;
  // calculate percentage value and store it in a global state var
  ADS_STATE_Storage_Usage = (currentVolume / maxVolume) * 100;
}


void Pause(string cause)
{
  if (cause == "Storage") {
    // print to Debug and Status LCD
    Debug("ADS_STATE_Storage_Usage: " + ADS_STATE_Storage_Usage.ToString(), true);
    PrintStatus("PAUSED! Check ADS_storage!", true);
    // Get rotor
    IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator;
    // stop rotor
	Rotor.ApplyAction("OnOff_Off");
    // save current ADS Paused State
    ADS_STATE_PAUSED_Storage = 1;
// TODO Stop Drill Blocks
  }
  
  if (cause == "FinishedLayer") {
    // print to Debug and Status LCD
    Debug("Pausing rotor, starting downward piston...", true);
    PrintStatus("Finished mining layer, moving borehead down.", true);
    // stop rotor
    IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator;
	Rotor.ApplyAction("OnOff_Off");
    // save current ADS Paused State
    ADS_STATE_PAUSED_FinishedLayer = 1;
  }
  
  if (cause == "FinishedAllLayers") {
    // print to Debug and Status LCD
    Debug("Reached max Piston D extension", true);
    PrintStatus("Finished mining all layers!", true);
    // stop rotor
	IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator;
	Rotor.ApplyAction("OnOff_Off");
    // save current ADS Paused State
    ADS_STATE_PAUSED_FinishedAllLayers = 1;
// TODO STOP Drills
  }
  
  // more pause operations with different actions needed?
}


void Resume(string cause)
{
  if (cause == "Storage") {
    // print to Debug and Status LCD
    Debug("ADS_STATE_Storage_Usage: " + ADS_STATE_Storage_Usage.ToString(), true);
    PrintStatus("Resumed! ADS_storage has space again!", true);
    // restart rotor
	IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator;
	Rotor.ApplyAction("OnOff_On");
    // save current ADS normal State
    ADS_STATE_PAUSED_Storage = 0;
//TODO Restart Drill Blocks
  }
  
  if (cause == "FinishedLayer") {
    // print to Debug and Status LCD
    Debug("Resuming rotor, finished downward piston...", true);
    PrintStatus("Finished moving borehead down, resuming rotation.", true);
      // restart rotor
	IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName("ADS_Rotor") as IMyMotorStator;
	Rotor.ApplyAction("OnOff_On");
    // save current ADS Paused State
    ADS_STATE_PAUSED_FinishedLayer = 0;
  }
  
  
  // more resume operations with different actions needed?
}


void Debug(string text, bool append) 
{ 
  // fetch Debug LCD Block object 
  IMyTextPanel ADS_LCD_Debug = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("ADS_Debug"); 
  // print text to Debug LCD 
  ADS_LCD_Debug.WritePublicText(text + "\n" , append); 
} 


void PrintStatus(string text, bool append) 
{ 
  // fetch Status LCD Block object 
  IMyTextPanel ADS_LCD_Status = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("ADS_Status"); 
  // print text to Status LCD 
  ADS_LCD_Status.WritePublicText(text + "\n" , append); 
}
