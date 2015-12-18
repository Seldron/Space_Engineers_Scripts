/*
######################################################################################
Space Engineers Piston Control Draft V1
Script can be used to control a piston by buttons or hotbar shortcuts.
The script doesn't need a timer, since the button pressed will initiate the movement.
It needs 2 arguments seperated by ,
First argument is either + or -
second argument is the piston name
######################################################################################
*/

/*
######################################################################################
Example Argument:
+,Piston1
-> will move Piston 1 by P_Extend m with a speed of P_Velocity.
######################################################################################
*/

// Options
float P_Extend = 0.25f;
float P_Velocity = 0.25f;

public void Main(string argument)
{
   // split argument for direction and piston name
   string[] args = argument.Split(',');
   string direction = args[0];
   string PistonName = args [1];

   // fetch piston
   IMyPistonBase Piston = GridTerminalSystem.GetBlockWithName(PistonName) as IMyPistonBase;
   
   // fetch current position
   double P_Pos = Convert.ToDouble((Piston.DetailedInfo.Remove(0,28)).TrimEnd('m'));

   // + movement
   if ((direction == "+")
   {
      float target = P_Pos + P_Extend;
      Piston.SetValue<float>("UpperLimit", target);
      Piston.SetValue("Velocity",P_Velocity);
   }

   // - movement
   if ((direction == "-")
   {
      float target = P_Pos - P_Extend;
      if (target < 0) { target = 0.0f; }
      Piston.SetValue<float>("UpperLimit", target);
      Piston.SetValue("Velocity",P_Velocity*-1);
   }
}