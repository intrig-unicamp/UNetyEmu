using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to create a PID controller to control the drone's position:
public class PID
{
    
    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private float previousError = 0f;
    private float integral = 0f;

    // -----------------------------------------------------------------------------------------------------
    // Method to compute the PID controller:
    
    public float Compute(float error, float Kp, float Ki, float Kd)
    {
        
        // Proportional term:
        float proportional = Kp * error;

        // Integral term:        
        integral += error * Time.fixedDeltaTime;
        float integralTerm = Ki * integral;

        // Derivative term:
        float derivative = (error - previousError) / Time.fixedDeltaTime;
        float derivativeTerm = Kd * derivative;

        // Update the previous error:
        previousError = error;

        // Return the PID controller output:
        return proportional + integralTerm + derivativeTerm;

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to reset the integral value of the controller:
    
    public void Reset()
    {
        integral = 0f; // Reset the integral value
    }

}
