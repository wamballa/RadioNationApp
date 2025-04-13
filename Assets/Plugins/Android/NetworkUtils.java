package com.wamballa.vlcwrapper;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;

public class NetworkUtils {
    public static boolean isOnline(Context context) {
        try {
            // Pings Google DNS to verify actual connectivity
            Process p = Runtime.getRuntime().exec("ping -c 1 8.8.8.8");
            int returnVal = p.waitFor();
            return (returnVal == 0);
        } catch (Exception e) {
            return false;
        }
    }
}

