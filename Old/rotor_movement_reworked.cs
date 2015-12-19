/*
######################################################################################
Space Engineers Rotor Control
Script can be used to control a rotor by buttons or hotbar shortcuts.
The script doesn't need a timer, since the button pressed will initiate the movement.
It needs 2 arguments seperated by ,
First argument is either + or -
second argument is the rotor name
######################################################################################
*/

/*
######################################################################################
Example Argument:
+,Rotor1
-> will move Rotor1 by R_Angle_Extend degrees with a speed of R_Velocity.
######################################################################################
*/

// Options
float R_Angle_Extend = 5.0f;
float R_Velocity = 0.5f;

public void Main(string argument)
{
   // split argument for direction and rotor name
   string[] args = argument.Split(',');
   string direction = args[0];
   string RotorName = args [1];

   // fetch rotor
   IMyMotorStator Rotor = GridTerminalSystem.GetBlockWithName(RotorName) as IMyMotorStator;
   
   // fetch current position
   float R_Angle = (float) Convert.ToDouble((Rotor.DetailedInfo.Remove(0,25)).TrimEnd('°'));

   // + movement
   if (direction == "+")
   {
      float target = R_Angle + R_Angle_Extend;
      Rotor.SetValue("UpperLimit", target);
      Rotor.SetValue("Velocity",R_Velocity);
   }

   // - movement
   if (direction == "-")
   {
      float target = R_Angle - R_Angle_Extend;
      Rotor.SetValue("LowerLimit", target);
      Rotor.SetValue("Velocity",R_Velocity*-1);
   }
}