// Pull in an array of blocks into base position
// block names
string Rotor1Name = "Welder_MainBoom";
string Rotor2Name = "Welder_Rotor";
string PistonName = "Welder_Piston";

public void Main(string argument)
{
   if (argument == "I")
   {
      // fetch Rotor1 Block	
      IMyMotorStator Rotor1 = GridTerminalSystem.GetBlockWithName(Rotor1Name) as IMyMotorStator;
      int Rotor1_Angle = Convert.ToInt32((Rotor1.DetailedInfo.Remove(0,25)).TrimEnd('°'));
      // fetch Rotor2 Block	
      IMyMotorStator Rotor2 = GridTerminalSystem.GetBlockWithName(Rotor2Name) as IMyMotorStator;
      int Rotor2_Angle = Convert.ToInt32((Rotor2.DetailedInfo.Remove(0,25)).TrimEnd('°'));
      // fetch Piston Block
      IMyPistonBase Piston = GridTerminalSystem.GetBlockWithName(PistonName) as IMyPistonBase;

      // pull in piston
      Piston.SetValue("Velocity",-0.1f);
      Piston.ApplyAction("OnOff_On");
      
      // turn rotor 1 to 0 angle
      if (Rotor1_Angle >= 3)
      {
         Rotor1.SetValue<float>("LowerLimit", 0.0f);
         Rotor1.SetValue("Velocity",-0.5f);
         Rotor1.ApplyAction("OnOff_On");
      } else {
         if (Rotor1_Angle <= -3)
         {
            Rotor1.SetValue<float>("UpperLimit", 0.0f);
            Rotor1.SetValue("Velocity",0.5f);
            Rotor1.ApplyAction("OnOff_On");
         }
      }
      
      // turn rotor 2 to 0 angle
      if (Rotor2_Angle >= 5)
      {
         Rotor2.SetValue<float>("LowerLimit", 0.0f);
         Rotor2.SetValue("Velocity",-0.5f);
         Rotor2.ApplyAction("OnOff_On");
      } else {
         if (Rotor2_Angle <= -5)
         {
            Rotor2.SetValue<float>("UpperLimit", 0.0f);
            Rotor2.SetValue("Velocity",0.5f);
            Rotor2.ApplyAction("OnOff_On");
         }
      }
   }
}
