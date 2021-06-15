public static class CelluloEnums
{
    public enum ConnectionStatus {
    /** Idle and not connected     */ ConnectionStatusDisconnected = 0,
    /** Actively trying to connect */ ConnectionStatusConnecting = 1,
    /** Connected                  */ ConnectionStatusConnected = 2,
    ConnectionStatusNumElements
    }
    public enum BatteryState {
        /** No charger present, battery draining                                */ BatteryStateDischarging = 0,
        /** No charger present, battery low, will shut down                     */ BatteryStateLow = 1,
        /** Charger present, battery charging                                   */ BatteryStateCharging = 2,
        /** Charger present, battery full                                       */ BatteryStateCharged = 3,
        /** Charger charging disabled, voltage too low or battery not present   */ BatteryStateShutdown = 4,
        /** Thermal fault or charge timeout                                     */ BatteryStateError = 5,
        BatteryStateNumElements
    }
    public  enum VisualEffect {
        /** Set all LED colors (value unused)                               */ VisualEffectConstAll = 0,
        /** Set one LED color (value is LED index)                          */ VisualEffectConstSingle =1,
        /** Alert animation for all LEDs (value unused)                     */ VisualEffectAlertAll = 2,
        /** Alert animation for one LED (value is LED index)                */ VisualEffectAlertSingle = 3,
        /** Static progress circularly (value 0-255 maps to 0-100%)         */ VisualEffectProgress = 4,
        /** Circular waiting/processing animation (value unused)            */ VisualEffectWaiting = 5,
        /** Point toward one direction (value 0-255 maps to 0-360 degrees)  */ VisualEffectDirection = 6,
        /** Alert forever (value*20 is LED on time in milliseconds)         */ VisualEffectBlink = 7,
        /** Breathe animation (value unused)                                */ VisualEffectBreathe = 8,
        /** Slower breathe-like animation (value unused)                    */ VisualEffectPulse = 9,
        VisualEffectNumElements
    }
    public enum Touch{
        /** Touch Released **/ TouchReleased=0,
        /** Touch Pressed **/ TouchBegan=1,
        /** Long Touched **/ LongTouch = 2
    }
}
