/*
######################################################################################
Space Engineers Fine Robot Control Script
Script can be used to control a robot arm.
Script has a Init section which needs to be Adjusted for your needs.
Use the Templates provided to set up your initial "Home" position of your robot arm.
Setup Buttons or Shortcut bar to run the script using the following arguments:

ARGUMENTS:
Action,Blockname,Extend,Velocity

Argument Descriptions:
Action - string
   R+   - moves a rotor in + direction
   R-   - moves a rotor in - direction
   P+   - moves a piston outwards
   P-   - moves a piston inwards
   Init - moves everything to predefined init positions, doesnt need other arguments
Blockname - string
   - The extaxt Blockname, "," is not allowed in blocknames!
Extend - float
   - angle or extend (amount to move)
Velocity - float
   - speed (m/s or rpm)
######################################################################################
Example Arguments:

R+,Rotor1,5,0.5
-> will move Rotor1 by 5 degrees in + direction with a speed of 0.5 rpm

P-,Piston1,0.25,0.25
-> will pull in Piston1 by 0.25 m with a speed of 0.25 meters per second

Init
-> will move every block defined in the INIT SECTION in the main function of this script
   into its specified "Home" positions

######################################################################################
*/

// dont change the following vars
string Rotor_Name = "will_be_overwritten";
float Rotor_InitPos = 0.0f;
float Rotor_InitVelocity = 0.5f;
string Piston_Name = "will_be_overwritten";
float Piston_InitPos = 0.0f;
float Piston_InitVelocity = 0.1f;

void Main(string argument)
{
   /*
   Init Functions, Adjust the following Block to your Initial State Needs.
   Delete Example Sections if you don't need them.'
   */
// ################# INIT SECTION START ##############################################
// ################# SETUP YOUR HOME POSITIONS HERE ##################################
   if (argument == "Init")
   {
      // move rotors to their init state
      /* TEMPLATE */
      /*
      Rotor_Name = "your rotors name";
      Rotor_InitPos = 0.0f; // init angle as a floating point variable
      Rotor_InitVelocity = 0.5f; // speed in RPM as a floating point variable
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_Velocity); // this function sets your wanted values
      */
      
      Rotor_Name = "Rotor1Name";
      Rotor_InitPos = 0.0f;
      Rotor_InitVelocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_InitVelocity);
      
      Rotor_Name = "Rotor2Name";
      Rotor_InitPos = 0.0f;
      Rotor_InitVelocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_InitVelocity);
      
      Rotor_Name = "Rotor3Name";
      Rotor_InitPos = 0.0f;
      Rotor_InitVelocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_InitVelocity);
      
      Rotor_Name = "Rotor4Name";
      Rotor_InitPos = 0.0f;
      Rotor_InitVelocity = 0.5f;
      InitRotor(Rotor_Name,Rotor_InitPos,Rotor_InitVelocity);


      // move pistons to their init state
      /* TEMPLATE */
      /*
      Piston_Name = "your pistons name";
      Piston_InitPos = 0.0f; // init position (m) as a floating point variable
      Piston_InitVelocity = 0.25f; // speed in meter per second as a floating point variable
      InitRotor(Piston_Name,Piston_InitPos,Piston_Velocity); // this function sets your wanted values
      */
      
      Piston_Name = "Piston 1";
      Piston_InitPos = 0.0f;
      Piston_InitVelocity = 0.25f;
      InitRotor(Piston_Name,Piston_InitPos,Piston_InitVelocity);
      
      Piston_Name = "Piston 2";
      Piston_InitPos = 0.0f;
      Piston_InitVelocity = 0.25f;
      InitRotor(Piston_Name,Piston_InitPos,Piston_InitVelocity);
      
// ################# INIT SECTION END ##############################################

   } else
   {
      // split arguments for movement
      string[] args = argument.Split(',');
      string Action = args[0];
      string BlockName = args[1];
      float Extend = (float) Convert.ToDouble(args[2]);
      float Velocity = (float) Convert.ToDouble(args[3]);
      
      // move a rotor in + direction
      if (Action == "R+")
      {
         MoveRotor(BlockName, "+", Extend, Velocity)
      }
      
      // move a rotor in - direction
      if (Action == "R-")
      {
         MoveRotor(BlockName, "-", Extend, Velocity)
      }
      
      // move a piston in + direction
      if (Action == "P+")
      {
         MovePiston(BlockName, "+", Extend, Velocity)
      }
      
      // move a piston in - direction
      if (Action == "P-")
      {
         MovePiston(BlockName, "-", Extend, Velocity)
      }
   }
}

void InitRotor(string RotorName, float InitPos, float Velocity)
{
   IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
   float Angle = (float) Convert.ToDouble((Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));
   float UL = InitPos + 3.0f;
   float LL = InitPos -3.0f;
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
   float UL = InitPos + 0.2f;
   float LL = InitPos - 0.2f;
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

void MoveRotor(string RotorName, string Direction, float AngleExtend, float Velocity)
{
   // fetch rotor
   IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
   
   // fetch current position
   float R_Angle = (float) Convert.ToDouble((Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));

   // + movement
   if (Direction == "+")
   {
      float target = R_Angle + AngleExtend;
      Rotor.SetValue("UpperLimit", target);
      Rotor.SetValue("Velocity",Velocity);
      Rotor.ApplyAction("OnOff_On");
   }

   // - movement
   if (Direction == "-")
   {
      float target = R_Angle - AngleExtend;
      Rotor.SetValue("LowerLimit", target);
      Rotor.SetValue("Velocity",Velocity*-1);
      Rotor.ApplyAction("OnOff_On");
   }
}

void MovePiston(string PistonName, string Direction, float Extend, float Velocity)
{
   // fetch piston
   IMyPistonBase Piston = GridTerminalSystem.GetBlockWithName(PistonName) as IMyPistonBase;
   
   // fetch current position
   float P_Pos = (float) Convert.ToDouble((Piston.DetailedInfo.Remove(0,28)).TrimEnd('m'));
   
   // + movement
   if (Direction == "+")
   {
      float target = P_Pos + Extend;
      Piston.SetValue("UpperLimit", target);
      Piston.SetValue("Velocity",P_Velocity);
      Piston.ApplyAction("OnOff_On");
   }

   // - movement
   if (Direction == "-")
   {
      float target = P_Pos - Extend;
      if (target < 0) { target = 0.0f; }
      Piston.SetValue("UpperLimit", target);
      Piston.SetValue("Velocity",P_Velocity*-1);
      Piston.ApplyAction("OnOff_On");
   }
}