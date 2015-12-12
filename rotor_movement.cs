// Space Engineers Rotor Control Draft V4
// Options
float angle_inc = 5.0f;

// block names
string RotorName = "Welder_MainBoom";
string TimerName = "Rotor_Program";
float Rotor_Velocity = 0.5f;

// stored vars
string last_arg = "";
float target;
bool CFinished = true;
bool CArg = true;
float CAngle;

public void Main(string argument)

{
   if (argument == "R") {
      last_arg = "";
      target = 0.0f;
      CFinished = true;
      CArg = true;
      CAngle = 0.0f;
   } else {
   
	if ((argument == "+") || (argument == "-")) {
		// fetch argument
		last_arg = argument;
		CArg = true;
	} else {
		CArg = false;
	}
	
	// fetch Rotor Block	
	IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
	// get current Limits
	var Rotor_UpperLimit = Rotor.GetValue<float>("UpperLimit"); 
  	var Rotor_LowerLimit = Rotor.GetValue<float>("LowerLimit");
	// get current_angle
	int Rotor_Angle = Convert.ToInt32((Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));
	if (CArg == true) {
		if(CFinished == true) {
			if (argument == "+") {
				last_arg = argument;
				CAngle = Rotor_Angle;
				target = Rotor_Angle + 5f;
				Rotor.SetValue<float>("UpperLimit", target);
				Rotor.SetValue("Velocity",Rotor_Velocity);
				CFinished = false;
			} else {
				last_arg = argument;
				CAngle = Rotor_Angle;
				target = Rotor_Angle - 5f;
				Rotor.SetValue<float>("LowerLimit", target);
				float t_vel = -1 * Rotor_Velocity;
				Rotor.SetValue("Velocity",t_vel);
				CFinished = false;
			}
			// restart timer
			IMyTimerBlock Timer = GridTerminalSystem.GetBlockWithName(TimerName)as IMyTimerBlock;
			Timer.GetActionWithName("TriggerNow").Apply(Timer); 
		} else {
			if (last_arg == "+") {
				if (CAngle + 5f <= target) {
					CFinished = true;
				}
			}
			if (last_arg == "-"){
				if (CAngle - 5f >= target) {
					CFinished = true;
				}
			}
		}
	}
   }
}
