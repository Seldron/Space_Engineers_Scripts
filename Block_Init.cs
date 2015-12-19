/* Script can be used to reset several pistons or rotors to their init position
Copy Template Blocks in main function for each block you want to set.
Adjust Values as you need them for each block.
*/

// dont change the following vars
string Rotor_Name = "will_be_overwritten";
float Rotor_InitPos = 0.0f;
float Rotor_Velocity = 0.5f;
string Piston_Name = "will_be_overwritten";
float Piston_InitPos = 0.0f;
float Piston_Velocity = 0.1f;

void Main(string argument)
{
   if (argument == "I")
   {
      // move rotors to their init state
      /* TEMPLATE */
      /*
      Rotor_Name = "your rotors name";
      Rotor_InitPos = 0.0f; // init angle as a floating point variable
      Rotor_Velocity = 0.5f; // speed in RPM as a floating point variable
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity); // this function sets your wanted values
      */
      
      Rotor_Name = "Rotor1Name";
      Rotor_InitPos = 0.0f;
      Rotor_Velocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity);
      
      Rotor_Name = "Rotor2Name";
      Rotor_InitPos = 0.0f;
      Rotor_Velocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity);
      
      Rotor_Name = "Rotor3Name";
      Rotor_InitPos = 0.0f;
      Rotor_Velocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity);
      
      Rotor_Name = "Rotor4Name";
      Rotor_InitPos = 0.0f;
      Rotor_Velocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity);


      // move pistons to their init state
      /* TEMPLATE */
      /*
      Piston_Name = "your pistons name";
      Piston_InitPos = 0.0f; // init position (m) as a floating point variable
      Piston_Velocity = 0.25f; // speed in meter per second as a floating point variable
      InitRotor(Piston_Name,Piston_InitPos,Piston_Velocity); // this function sets your wanted values
      */
      
      Piston_Name = "Piston 1";
      Piston_InitPos = 0.0f;
      Piston_Velocity = 0.25f;
      InitRotor(Piston_Name,Piston_InitPos,Piston_Velocity);
      
      Piston_Name = "Piston 2";
      Piston_InitPos = 0.0f;
      Piston_Velocity = 0.25f;
      InitRotor(Piston_Name,Piston_InitPos,Piston_Velocity);

   }
}

void InitRotor(string RotorName, float InitPos, float Velocity)
{
   IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
   float Angle = (float) Convert.ToDouble((Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));
   float UL = InitPos + 3;
   float LL = InitPos -3;
   float Velocity_Inverse = -1 * Velocity;
   if (Angle >= UL)
   {
      Rotor.SetValue("LowerLimit", InitPos);
      Rotor.SetValue("Velocity",Velocity_Inverse);
      Rotor.ApplyAction("OnOff_On");
   }
   if (Angle <= LL)
   {
      Rotor.SetValue("UpperLimit", InitPos);
      Rotor.SetValue("Velocity",Velocity);
      Rotor.ApplyAction("OnOff_On");
   }
}

void InitPiston(string PistonName, float InitPos, float Velocity)
{
   IMyPistonBase Piston = GridTerminalSystem.GetBlockWithName(PistonName) as IMyPistonBase;
   float Pos = (float) Convert.ToDouble((Piston.DetailedInfo.Remove(0,28)).TrimEnd('m'));
   float UL = InitPos + 0.2;
   float LL = InitPos - 0.2;
   float Velocity_Inverse = -1 * Velocity;
   if (Pos >= UL)
   {
      Piston.SetValue("LowerLimit", InitPos);
      Piston.SetValue("Velocity",Velocity_Inverse);
      Piston.ApplyAction("OnOff_On");
   }
   if (Pos <= LL)
   {
      Piston.SetValue("UpperLimit", InitPos);
      Piston.SetValue("Velocity",Velocity);
      Piston.ApplyAction("OnOff_On");
   }  
}
