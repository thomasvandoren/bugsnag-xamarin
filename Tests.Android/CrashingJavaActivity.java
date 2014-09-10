package bugsnag.test;

public class CrashingJavaActivity
    extends android.app.Activity
{
    protected void onCreate(android.os.Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(12312345);
    }
}
