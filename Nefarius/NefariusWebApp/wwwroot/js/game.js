var app = new angular.module("nefarius", ['dnd']);

let hubUrl = 'http://pc-citrgm06.vsegei.ru:2047/game';
let httpConnection = new signalR.HttpConnection(hubUrl, {
    transport: signalR.TransportType.LongPolling
});
let hubConnection = new signalR.HubConnection(httpConnection);

//let hubConnection = new signalR.HubConnection(hubUrl, { transport: signalR.TransportType.WebSocket }); // For standalone

hubConnection.start();

//// Start the connection.
//let hubConnection = startConnection(hubUrl)
//.then(function (connection) {
//    console.log('connection started');
//    init();
//})
//.catch(error => {
//    console.error(error.message);
//});
//// Starts a connection with transport fallback - if the connection cannot be started using
//// the webSockets transport the function will fallback to the serverSentEvents transport and
//// if this does not work it will try longPolling. If the connection cannot be started using
//// any of the available transports the function will return a rejected Promise.
//function startConnection(url, configureConnection) {
//    return function start(transport) {
//        console.log(`Starting connection using ${signalR.TransportType[transport]} transport`)
//        var connection = new signalR.HubConnection(url, { transport: transport });
//        if (configureConnection && typeof configureConnection === 'function') {
//            configureConnection(connection);
//        }
//        return connection.start()
//            .then(function () {
//                return connection;
//            })
//            .catch(function (error) {
//                console.log(`Cannot start the connection use ${signalR.TransportType[transport]} transport. ${error.message}`);
//                if (transport !== signalR.TransportType.LongPolling) {
//                    return start(transport + 1);
//                }
//                return Promise.reject(error);
//            });
//    }(signalR.TransportType.WebSockets);
//}

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
        0: "Init",
        1: "Turning",
        2: "Spying",
        3: "Spy",
        4: "Invent",
        5: "Inventing",
        6: "Research",
        7: "Work",
        8: "Scoring",
        9: "Win"
    }

    $scope.actions = [
        {
            description: 1,
            name: "Шпионаж",
            color: "#ff0000"
        },
        {
            description: 2,
            name: "Изобретение",
            color: "#00caff"
        },
        {
            description: 3,
            name: "Исследование",
            color: "#ffff00"
        },
        {
            description: 4,
            name: "Работа",
            color: "#5db700"
        }
    ];

    $scope.spyZones = [
        {
            bg: 'sprites/SPY.png',
            value: 1
        },
        {
            bg: 'sprites/INVENT.png',
            value: 2
        },
        {
            bg: 'sprites/STUDY.png',
            value: 3
        },
        {
            bg: 'sprites/WORK.png',
            value: 4
        },
    ]


    $scope.getPlayerCurrentState = function () {
        var msg = '';

        switch ($scope.currentGameState) {
            case "Init":
                msg = 'Ждем начала игры';
                $scope.waiting = true;
                break;
            case "Turning":
                if (!$scope.turnApllyed) {
                    msg = 'Выбери действие';
                    $scope.waiting = true;
                } else {
                    msg = 'Ждем других игроков';
                    $scope.waiting = false;
                }
                break;
            case "Spying":
                msg = 'Расчет денег за шпионов';
                $scope.waiting = true;
                break;
            case "Spy":
                if ($scope.turnSelected == 1) {
                    msg = 'Поставь шпиона';
                    $scope.waiting = true;
                }
                break;
            case "Invent":
                if ($scope.turnSelected == 2 && !$scope.inventSelected) {
                    msg = 'Выбери изобретение';
                    $scope.waiting = true;
                } else {
                    msg = 'Ждем выбора изобретений';
                    $scope.waiting = false;
                }
                break;
            case "Inventing":
                msg = 'Применение эффектов';
                $scope.waiting = true;
                break;
            case "Research":
                msg = 'Проводим исследования';
                $scope.waiting = true;
                break;
            case "Work":
                msg = 'Работаем';
                $scope.waiting = true;
                break;
            case "Scoring":
                msg = 'Считаем очки';
                $scope.waiting = true;
                break;
            case "Win":
                msg = 'Есть победитель';
                $scope.waiting = true;
                break;
            default:
                msg = "Роман что то не доделал";
        }

        return msg;
    }

    $scope.currentGameState = 0;
    $scope.playerJoined = false;
    $scope.draggedSpy = null;
    $scope.inventSelectedForDrop = null;
    $scope.dropedInvents = [];
    $scope.cardsToDrop = 2;
    $scope.prevState = '';

    $scope.enemys = [];
    $scope.player = {
        name: ""
    };
    
    function setDataValuesToDefault(){
        $scope.selectedZone = null;
        $scope.cardDropEvent = false;
        $scope.turnSelected = 0;
        $scope.inventSelected = 0;
        $scope.turnApllyed = false;
    }
    
    setDataValuesToDefault();

    $scope.selectAction = function (card) {
        $scope.turnSelected = card.description;
        $scope.actions.forEach(function (card) {
            card.active = false;
        });
        card.active = true;
    }

    $scope.selectInvent = function (card) {
        $scope.inventSelected = card;
        $scope.player.inventions.forEach(function (card) {
            card.active = false;
        });
        card.active = true;

    }

    $scope.applyInvent = function (invent) {
        hubConnection.invoke("Invent", invent.id);
    }

    $scope.join = function (name, evt) {
        if (!name || (evt && evt.keyCode != 13))
            return;

        $scope.playerJoined = true;
        hubConnection.invoke("Join", name);
    };

    $scope.selectZone = function (value) {
        $scope.selectedZone = value;
    }

    $scope.selectSpyByDrag = function (spy) {
        $scope.draggedSpy = spy;
    }

    $scope.onSpyDrop = function (spy, zone) {
        if (spy == undefined || zone == undefined)
            return;

        hubConnection.invoke("Spy", zone, spy);
        $scope.draggedSpy = null;
        $scope.selectedZone = null;
    }

    $scope.onInventDrop = function (invent, count) {
        var _fromArr = $scope.player.inventions;
        var _toArr = $scope.dropedInvents;

        if (!count) { // возврат в руку
            _toArr = $scope.player.inventions;
            _fromArr = $scope.dropedInvents;
        } else if (_toArr.length + 1 > count) { // Если кидаем на сброс и превысили количество сбрасываемых карт
            var _overInvent = _toArr.shift();

            _fromArr.push(_overInvent);
        }

        _fromArr.splice($scope.player.inventions.indexOf(invent), 1);
        _toArr.push(invent);
    }

    $scope.applyDrop = function () {
        $scope.dropedInvents.forEach($scope.applyInvent);
        $scope.cardDropEvent = false;
    }

    $scope.activateInventDrop = function (count) {
        $scope.cardDropEvent = true;
        $scope.inventSelectedForDrop = null;
        $scope.dropedInvents = [];
        $scope.cardsToDrop = count;
    }

    $scope.tryEffect = function (effect) {
        if (effect.direction == "drop") {
            switch (effect.item) {
                case "invention":
                    $scope.activateInventDrop(effect.count);
                    break;
            }
        }
    }

    $scope.selectInventByDrag = function (invent) {
        $scope.inventSelectedForDrop = invent;
    }

    hubConnection.on("PlayerData", function (data) {
        console.log(data.effectQueue);
        if (data.effectQueue.length) {
            $scope.tryEffect(data.effectQueue[0]);
        }
        $scope.player = data;
        $scope.$apply();
    });

    hubConnection.on("StateChanged", function (data) {
        $scope.enemys = data.players;
        $scope.currentGameState = $scope.gameState[data.state];

        if ($scope.prevState != $scope.currentGameState) {
            if ($scope.currentGameState == 'Turning') {
                setDataValuesToDefault();
            }
            $scope.prevState = $scope.currentGameState;
        }


        $scope.$apply();
    });

    $scope.doTurn = function (turn) {
        hubConnection.invoke("Turn", turn);
        $scope.turnApllyed = true;
    };


    $scope.startGame = function () {
        hubConnection.invoke("Begin");
    };

    $scope.endGame = function () {
        hubConnection.invoke("End");
    };
});
