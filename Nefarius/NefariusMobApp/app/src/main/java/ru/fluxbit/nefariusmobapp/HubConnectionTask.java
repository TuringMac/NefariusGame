package ru.fluxbit.nefariusmobapp;

import android.os.AsyncTask;

import com.microsoft.signalr.HubConnection;

public class HubConnectionTask extends AsyncTask<HubConnection, Void, Void> {
    @Override
    protected void onPreExecute() {
        super.onPreExecute();
    }

    @Override
    protected Void doInBackground(HubConnection... hubConnections) {
        HubConnection hubConnection = hubConnections[0];
        hubConnection.start().blockingAwait();
        return null;
    }
}
