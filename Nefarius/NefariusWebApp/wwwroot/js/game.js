var app = new angular.module("nefarius", ['dnd']);

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
        {description: 1, name: "Шпионаж", color: "#ff0000"},
        {description: 2, name: "Изобретение", color: "#00caff"},
        {description: 3, name: "Исследование", color: "#ffff00"},
        {description: 4, name: "Работа", color: "#5db700"}
    ];
    
    $scope.spyZones = [
        {bg: 'sprites/SPY.png', value: 1},
        {bg: 'sprites/INVENT.png', value: 2},
        {bg: 'sprites/STUDY.png', value: 3},
        {bg: 'sprites/WORK.png', value: 4},
    ]
    
    $scope.currentGameState = 0;
    $scope.playerJoined = false;
    $scope.draggedSpy = null;
    $scope.selectedZone = null;
    $scope.values=[0,1,2,3];

    $scope.enemys = [];
    $scope.turnSelected = 0;
    $scope.inventSelected = 0;
    $scope.player = {
        name: ""
    };
    
    $scope.selectAction = function(card){
        $scope.turnSelected = card.description;
        $scope.actions.forEach(function(card){ card.active = false; });
        card.active = true;
    }
    
    $scope.selectInvent = function(card){
        $scope.inventSelected = card;
        $scope.player.inventions.forEach(function(card){ card.active = false; });
        card.active = true;
        
    }
    
    $scope.applyInvent = function(){
        hubConnection.invoke("Invent", $scope.inventSelected.id);
    }
    
    $scope.join = function(name,evt){
        if(!name || (evt && evt.keyCode != 13))
            return;

        $scope.playerJoined = true;
        hubConnection.invoke("Join", name);
    };
    
    $scope.selectZone = function(value){
        $scope.selectedZone = value;
    }
    
    $scope.selectSpyByDrag = function(spy){
        $scope.draggedSpy = spy;
    }
    
    $scope.onSpyDrop = function(spy,zone){
        hubConnection.invoke("Spy", zone,spy);
        $scope.draggedSpy = null;
        $scope.selectedZone = null;
    }

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
