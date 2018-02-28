var app = new angular.module("nefarius", []);

let hubUrl = '/game';
let httpConnection = new signalR.HttpConnection(hubUrl, {
    transport: signalR.TransportType.LongPolling
});
//let httpConnection = new signalR.HttpConnection(hubUrl); // For Kestrel
let hubConnection = new signalR.HubConnection(httpConnection);

app.controller("hub", function ($scope) {
    $scope.colors = {
        1: "#FF3F00",
        2: "#FFC700",
        3: "#00A000",
        4: "#0000DD",
        5: "#AA00AA",
        6: "#AA6600"
    };

    $scope.gameState = {
        0 : "Init",
        1 : "Turning",
        2 : "Spying",
        3 : "Spy",
        4 : "Invent",
        5 : "Inventing",
        6 : "Research",
        7 : "Work",
        8 : "Scoring",
        9 : "Win"
    }
    
    $scope.actions = [
        {avatarInfo: 1, title: "Шпионаж", color: "#ff0000"},
        {avatarInfo: 2, title: "Изобретение", color: "#00caff"},
        {avatarInfo: 3, title: "Исследование", color: "#ffff00"},
        {avatarInfo: 4, title: "Работа", color: "#5db700"}
    ];
    
    $scope.currentGameState = 0;
    $scope.playerJoined = false;


    $scope.enemys = [];
    $scope.turnSelected = 0;
    $scope.player = {
        name: ""
    };
    
    $scope.selectAction = function(card){
        turnSelected = card.avatarInfo;
        $scope.actions.forEach(function(card){ card.active = false; });
        card.active = true;
    }
    
    $scope.join = function(name){
        if(!name)
            return;

        $scope.playerJoined = true;
        hubConnection.invoke("Join", name);
    };

    hubConnection.on("PlayerData", function (data) {
        $scope.player = data;
        $scope.$apply();
    });

    hubConnection.on("StateChanged", function (data) {
        $scope.enemys = data.players;
        $scope.currentGameState = $scope.gameState[data.state];
        $scope.$apply();
    });

    $scope.doTurn = function (turn) {
        hubConnection.invoke("Turn", turn);
    };

    $scope.startGame = function () {
        hubConnection.invoke("Begin");
    };
});

//hubConnection.on("Send", function (data) {
//    let elem = document.createElement("p");
//    elem.appendChild(document.createTextNode(data));
//    let firstElem = document.getElementById("chatroom").firstChild;
//    document.getElementById("chatroom").insertBefore(elem, firstElem);
//});
//hubConnection.on("PlayerJoined", function (data) {
//    let elem = document.createElement("p");
//    elem.appendChild(document.createTextNode(data + " вошел в игру"));
//    let firstElem = document.getElementById("chatroom").firstChild;
//    document.getElementById("chatroom").insertBefore(elem, firstElem);
//});
//hubConnection.on("StateChanged", function (data) {
//    let elem = document.createElement("p");
//    elem.appendChild(document.createTextNode(data));
//    let firstElem = document.getElementById("chatroom").firstChild;
//    document.getElementById("chatroom").insertBefore(elem, firstElem);
//});
//hubConnection.on("AskTurn", function (data) {
//    // suspend thread, lol
//});
//// ------------------------------------------------------------------------
//document.getElementById("btnJoin").addEventListener("click", function (e) {
//    let name = document.getElementById("name").value;
//    hubConnection.invoke("Join", name);
//});
//document.getElementById("btnBegin").addEventListener("click", function (e) {
//    hubConnection.invoke("Begin");
//});
//document.getElementById("btnAct0").addEventListener("click", function (e) {
//    hubConnection.invoke("Turn", 0);
//});
//document.getElementById("btnAct1").addEventListener("click", function (e) {
//    hubConnection.invoke("Turn", 1);
//});
//document.getElementById("btnAct2").addEventListener("click", function (e) {
//    hubConnection.invoke("Turn", 2);
//});
//document.getElementById("btnAct3").addEventListener("click", function (e) {
//    hubConnection.invoke("Turn", 3);
//});
//document.getElementById("btnAct4").addEventListener("click", function (e) {
//    hubConnection.invoke("Turn", 4);
//});
hubConnection.start();
