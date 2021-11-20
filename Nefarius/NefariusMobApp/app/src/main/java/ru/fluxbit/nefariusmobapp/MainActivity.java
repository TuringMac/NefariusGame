package ru.fluxbit.nefariusmobapp;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {
    private String url_game = "https://nefarius.fluxbit.ru/game";
    private String ConnId = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        HubConnection hubConnection = HubConnectionBuilder.create(url_game).build();
        Button sendButton = (Button)findViewById(R.id.btn_join);
        TextView textView = (TextView)findViewById(R.id.tvMain);
        List<String> messageList = new ArrayList<String>();

        hubConnection.on("PlayerData", (data)-> {
            System.out.println("Player: "+data.Name+" Score: "+data.Score);
            //Log.println(Log.DEBUG, "SignalR", data);
        }, Player.class);

        hubConnection.on("StateChanged", (data)-> {
            System.out.println("Players: " + data.players.length + " State: "+data.state + " Move: "+data.move+ " Table: "+data.table);
            //Log.println(Log.DEBUG, "SignalR", data);
        }, GameData.class);

//        hubConnection.on("StateChanged", (message)-> {
//            runOnUiThread(new Runnable() {
//                @Override
//                public void run() {
//                    Log.println(Log.DEBUG, "SignalR", message);
//                }
//            });
//        }, String.class);

        hubConnection.on("TableList", (data)-> {
            System.out.println("Tables count: "+data.length);
            //Log.println(Log.DEBUG, "SignalR", data);
        }, Table[].class);

        sendButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    hubConnection.send("Join", "aaa","mob player");
                    openGameActivity();
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        });

        new HubConnectionTask().execute(hubConnection);
    }

    public void openGameActivity(){
        Intent intent = new Intent(this, GameActivity.class);
        startActivity(intent);
    }
}