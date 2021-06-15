using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
// ReSharper disable InconsistentNaming

public class Cellulo {
    // id inside the library
    private long id;
    //Properties
    public bool kidnapped;
    public int[] touch;
    public string macAddr;
    public bool isShuttingDown;
    public float goalX,goalY,goalTheta,goalVx,goalVy,goalW,goalV;

    // constructor: connect to robot
    public Cellulo()
    {
        id = newRobotFromPool();
        if(id == 0) {
            Debug.Log("WARNING: using zero pointer as a Cellulo object. It means that Cellulo object creation failed.");
            return;
        }
        touch = new int[6] {0,0,0,0,0,0};
    }

    // disconnect from robot on exit
    ~Cellulo() {
        destroyRobot(id);
    }

    // INIT Library, must call a bit before connecting
    [DllImport ("cellulo-library")]
    public static extern void initialize();

    // DEINIT Library, must call at quitting
    [DllImport ("cellulo-library")]
    public static extern void deinitialize();

    // number of robots in the pool
    [DllImport ("cellulo-library")]
    public static extern long totalRobots();

    // number of robots available (not used in Unity yet)
    [DllImport ("cellulo-library")]
    public static extern long robotsRemaining();

    // Cellulo methods export below...
    [DllImport ("cellulo-library")]
    private static extern long newRobotFromPool();
    [DllImport ("cellulo-library")]
    private static extern void destroyRobot(long id);
    [DllImport ("cellulo-library")]
    private static extern void setGoalVelocity(long robot, float vx, float vy, float w);
    public void setGoalVelocity(float vx, float vy, float w) {
        if (IsIdBad()) return;
        setGoalVelocity(id, vx, vy, w);
    }

    [DllImport ("cellulo-library")]
    private static extern void setGoalPose(long robot, float x, float y, float theta, float v, float w);
    public void setGoalPose(float x, float y, float theta, float v, float w) {
        if (IsIdBad()) return;
        setGoalPose(id, x, y, theta, v, w);
    }

    [DllImport ("cellulo-library")]
    private static extern void setGoalPosition(long robot, float x, float y, float v);
    public void setGoalPosition(float x, float y, float v) {
        if (IsIdBad()) return;
        setGoalPosition(id, x, y, v);
    }

    [DllImport ("cellulo-library")]
    private static extern void clearTracking(long robot);
    public void clearTracking() {
        if (IsIdBad()) return;
        clearTracking(id);
    }

    [DllImport ("cellulo-library")]
    private static extern void clearHapticFeedback(long robot);
    public void clearHapticFeedback() {
        if (IsIdBad()) return;
        clearHapticFeedback(id);
    }

    [DllImport ("cellulo-library")]
    private static extern void setVisualEffect(long robot, long effect, long r, long g, long b, long value);
    public void setVisualEffect(long effect, long r, long g, long b, long value) {
        if (IsIdBad()) return;
        setVisualEffect(id, effect, r, g, b, value);
    }


    [DllImport ("cellulo-library")]
    private static extern void setCasualBackdriveAssistEnabled(long robot, long enabled);
    public void setCasualBackdriveAssistEnabled(long enabled) {
        if (IsIdBad()) return;
        setCasualBackdriveAssistEnabled(id, enabled);
    }

    [DllImport ("cellulo-library")]
    private static extern void setHapticBackdriveAssist(long robot, float xAssist, float yAssist, float thetaAssist);
    public void setHapticBackdriveAssist(float xAssist, float yAssist, float thetaAssist) {
        if (IsIdBad()) return;
        setHapticBackdriveAssist(id, xAssist, yAssist, thetaAssist);
    }

    [DllImport ("cellulo-library")]
    private static extern void reset(long robot);
    public void reset() {
        if (IsIdBad()) return;
        reset(id);
    }

    [DllImport ("cellulo-library")]
    private static extern void simpleVibrate(long robot, float iX, float iY, float iTheta, long period, long duration);
    public void simpleVibrate(float iX, float iY, float iTheta, long period, long duration) {
        if (IsIdBad()) return;
        simpleVibrate(id, iX, iY, iTheta, period, duration);
    }

    [DllImport ("cellulo-library")]
    private static extern float getX(long robot);
    public float getX() {
        if (IsIdBad()) return(0.0f);
        return getX(id);
    }

    [DllImport ("cellulo-library")]
    private static extern float getY(long robot);
    public float getY() {
        if (IsIdBad()) return(0.0f);
        // Unity's y is up
        return getY(id);
    }

    [DllImport ("cellulo-library")]
    private static extern float getTheta(long robot);
    public float getTheta() {
        if (IsIdBad()) return(0.0f);
        return getTheta(id);
    }

    [DllImport ("cellulo-library")]
    private static extern long getKidnapped(long robot);
    public bool getKidnapped() {
        if (IsIdBad()) return false;
        return getKidnapped(id) > 0;
    }

    [DllImport ("cellulo-library")]
    private static extern int getBatteryState(long robot);
    public int getBatteryState() {
        if (IsIdBad()) return -1;
        return getBatteryState(id);
    }

    [DllImport ("cellulo-library")]
    private static extern int getLastTimeStamp(long robot);
    public int getLastTimeStamp() {
        if (IsIdBad()) return -1;
        return getLastTimeStamp(id);
    }

    [DllImport ("cellulo-library", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getMacAddr(long robot);
    public string getMacAddr() {
        if (IsIdBad()) return "";
        IntPtr intptr = (getMacAddr(id));
        macAddr = Marshal.PtrToStringAnsi(intptr,17);
        // Debug.Log("getMacAddr : "+macAddr );
        return macAddr;
    }

    [DllImport ("cellulo-library")]
    private static extern int getConnectionStatus(long robot);
    public int getConnectionStatus() {
        if (IsIdBad()) return -1;
        return getConnectionStatus(id);
    }

    // defining the type
    public delegate void noArgCallbackType();

    // our variable (need to remember the value, otherwise it gets garbage collected)
    // See https://forum.unity.com/threads/native-plugin-c-interop-crash-in-callback.337178/
    private noArgCallbackType kidnappedCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setKidnappedCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setKidnappedCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        kidnappedCallback = callback;
        setKidnappedCallback(id, kidnappedCallback);
    }

    ////////////////////////TOUCH/////////////////////////////////////
    // defining the type
    public delegate void touchCallbackType(int key);
    private touchCallbackType touchBeganCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setTouchBeganCallback(long robot, touchCallbackType callback);

    // public function in C# to set the callback
    public void setTouchBeganCallback(touchCallbackType callback) {
        if (IsIdBad()) return;
        touchBeganCallback = callback;
        setTouchBeganCallback(id, touchBeganCallback);
    }
    private touchCallbackType touchReleasedCallback;
    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setTouchReleasedCallback(long robot, touchCallbackType callback);

    // public function in C# to set the callback
    public void setTouchReleasedCallback(touchCallbackType callback) {
        if (IsIdBad()) return;
        touchReleasedCallback = callback;
        setTouchReleasedCallback(id, touchReleasedCallback);
    }

    private touchCallbackType longTouchCallback;
    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setLongTouchCallback(long robot, touchCallbackType callback);

    // public function in C# to set the callback
    public void setLongTouchCallback(touchCallbackType callback) {
        if (IsIdBad()) return;
        longTouchCallback = callback;
        setLongTouchCallback(id, longTouchCallback);
    }

    ////////////////////////Low Battery///////////////////////////////
    private noArgCallbackType lowBatteryCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setLowBatteryCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setLowBatteryCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        lowBatteryCallback = callback;
        setLowBatteryCallback(id, lowBatteryCallback);
    }

    ////////////////////////Shutting Down///////////////////////////////
    private noArgCallbackType shuttingDownCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setShuttingDownCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setShuttingDownCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        shuttingDownCallback = callback;
        setShuttingDownCallback(id, shuttingDownCallback);
    }

    ////////////////////////Boot Completed///////////////////////////////
    private noArgCallbackType bootCompletedCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setBootCompletedCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setBootCompletedCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        bootCompletedCallback = callback;
        setBootCompletedCallback(id, bootCompletedCallback);
    }

    ////////////////////////Tracking Goal Reached///////////////////////////////
    private noArgCallbackType trackingGoalReachedCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setTrackingGoalReachedCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setTrackingGoalReachedCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        trackingGoalReachedCallback = callback;
        setTrackingGoalReachedCallback(id, trackingGoalReachedCallback);
    }

    ////////////////////////Pose Changed///////////////////////////////
    public delegate void threeArgsCallbackType(float x,float y, float theta);
    private noArgCallbackType poseChangedCallback;

    // function from C to set the callback
    [DllImport ("cellulo-library")]
    private static extern void setPoseChangedCallback(long robot, noArgCallbackType callback);

    // public function in C# to set the callback
    public void setPoseChangedCallback(noArgCallbackType callback) {
        if (IsIdBad()) return;
        poseChangedCallback = callback;
        setPoseChangedCallback(id, poseChangedCallback);
    }

    private bool IsIdBad()
    {
        if (id == 0)
        {
            Debug.Log("Robot is broken (connection to pool failed). Cannot do API call");
            return true;
        }

        return false;
    }
}
