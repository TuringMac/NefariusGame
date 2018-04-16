var app = new angular.module("nefarius", ['dnd']);

let hubUrl = localStorage.getItem('hubUrl') || '/game';

let hubConnection = new signalR.HubConnection(hubUrl);
//let hubConnection = new signalR.HubConnection(hubUrl, { transport: signalR.TransportType.LongPolling }); // For Win7 IIS
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
            color: "#ff0000",
            type: "action"
        },
        {
            description: 2,
            name: "Изобретение",
            color: "#00caff",
            type: "action"
        },
        {
            description: 3,
            name: "Исследование",
            color: "#ffff00",
            type: "action"
        },
        {
            description: 4,
            name: "Работа",
            color: "#5db700",
            type: "action"
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

    function getPlayerActionByEffect() {
        var effect = $scope.player.effectQueue[0];
        var action = "none";

        if (effect.direction == "drop") {
            switch (effect.item) {
                case "invention":
                    action = "dropInvent";
                    break;
                case "spy":
                    action = "dropSpy";
                    break;
            }
        }
        if (effect.direction == "get") {
            switch (effect.item) {
                case "spy":
                    action = "getSpy";
                    break;
            }
        }

        return action;
    }


    $scope.getPlayerCurrentState = function () {
        var msg = '';
        var hintPosition = 'player';

        switch ($scope.currentGameState) {
            case "Init":
                msg = 'Ждем начала игры';
                $scope.waiting = true;
                hintPosition = 'player';
                break;
            case "Turning":
                if (!$scope.turnApllyed) {
                    msg = 'Выбери действие';
                    hintPosition = 'actions';
                    $scope.waiting = true;
                } else {
                    msg = 'Ждем других игроков';
                    hintPosition = 'player';
                    $scope.waiting = false;
                }
                break;
            case "Spying":
                msg = 'Расчет денег за шпионов';
                hintPosition = 'player';
                $scope.waiting = true;
                break;
            case "Spy":
                if ($scope.player.action == 1) {
                    msg = 'Поставь шпиона';
                    hintPosition = 'player';
                    $scope.waiting = true;
                }
                break;
            case "Invent":
                if ($scope.player.action == 2 && !$scope.inventSelected) {
                    msg = 'Выбери изобретение';
                    hintPosition = 'inventions';
                    $scope.waiting = true;
                } else {
                    msg = 'Ждем выбора изобретений';
                    hintPosition = 'player';
                    $scope.waiting = false;
                }
                break;
            case "Inventing":
                var action = getPlayerActionByEffect();

                switch (action) {
                    case "getSpy":
                        msg = 'Поставь шпиона';
                        hintPosition = 'player';
                        $scope.waiting = true;
                        break;
                    case "dropSpy":
                        msg = 'Убери шпиона';
                        hintPosition = 'table';
                        $scope.waiting = true;
                        break;
                    case "dropInvent":
                        msg = 'Скидывай изобретения';
                        hintPosition = 'inventions';
                        $scope.waiting = true;
                        break;
                    case "none":
                        msg = 'Применение эффетов,ждем остальных';
                        hintPosition = 'player';
                        $scope.waiting = false;
                }
                break;
            case "Research":
                msg = 'Проводим исследования';
                hintPosition = 'player';
                $scope.waiting = true;
                break;
            case "Work":
                msg = 'Работаем';
                hintPosition = 'player';
                $scope.waiting = true;
                break;
            case "Scoring":
                msg = 'Считаем очки';
                hintPosition = 'player';
                $scope.waiting = true;
                break;
            case "Win":
                msg = 'Есть победитель';
                hintPosition = 'player';
                $scope.waiting = true;
                break;
            default:
                hintPosition = 'player';
                msg = "Роман что то не доделал";
        }

        $scope.hintPosition = hintPosition;
        $scope.hintMsg = msg;

        return msg;
    }

    $scope.currentGameState = 0;
    $scope.playerJoined = false;
    $scope.spyDropEvent = false;
    $scope.draggedSpy = null;
    $scope.spyToDrop = 0;
    $scope.spyAddEvent = false;
    $scope.spyToAdd = 0;
    $scope.inventSelectedForDrop = null;
    $scope.dropedInvents = [];
    $scope.cardsToDrop = 2;
    $scope.prevState = '';

    $scope.enemys = [];
    $scope.player = {
        name: ""
    };

    function setDataValuesToDefault() {
        $scope.selectedZone = null;
        $scope.selectedCard = null;
        $scope.cardDropEvent = false;
        $scope.turnApllyed = false;
    }

    setDataValuesToDefault();

    function cardCanBeSelected(card) {
        if (card.type && card.type == 'action') {
            if ($scope.currentGameState != 'Turning')
                return false;
        } else {
            if ($scope.currentGameState != 'Invent')
                return false;
        }

        return true;
    }

    $scope.selectCard = function (card) {
        if (cardCanBeSelected(card)) {
            $scope.selectedCard = card;
        }
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
        if ((!$scope.spyDropEvent && !$scope.spyAddEvent) || spy == undefined || zone == undefined)
            return;

        hubConnection.invoke("Spy", zone, spy);
        if (zone == 0)
            $scope.spyToAdd--;
        else
            $scope.spyToDrop--;

        if ($scope.spyToDrop == 0)
            $scope.spyDropEvent = false;

        if ($scope.spyToAdd == 0)
            $scope.spyAddEvent = false;

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
        $scope.dropedInvents.forEach($scope.sendDropInvent);
        $scope.cardDropEvent = false;
    }

    $scope.activateInventDrop = function (count) {
        $scope.cardDropEvent = true;
        $scope.inventSelectedForDrop = null;
        $scope.dropedInvents = [];
        $scope.cardsToDrop = count;
    }

    $scope.activateSpyDrop = function (count) {
        $scope.spyDropEvent = true;
        $scope.draggedSpy = null;
        $scope.spyToDrop = count;
    }

    $scope.activateSpyAdd = function (count) {
        $scope.spyAddEvent = true;
        $scope.spyToAdd = count;
    }

    $scope.tryEffect = function (effect) {
        if (effect.direction == "drop") {
            switch (effect.item) {
                case "invention":
                    $scope.activateInventDrop(effect.count);
                    break;
                case "spy":
                    $scope.activateSpyDrop(effect.count);
                    break;
            }
        }
        if (effect.direction == "get") {
            switch (effect.item) {
                case "spy":
                    $scope.activateSpyAdd(effect.count);
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
            if ($scope.currentGameState == 'Invent' && $scope.player.action == 2) {
                setDataValuesToDefault();
            }
            if ($scope.currentGameState == 'Spy' && $scope.player.action == 1) {
                $scope.activateSpyAdd(1);
            }
            $scope.prevState = $scope.currentGameState;
        }

        $scope.$apply();
    });

    hubConnection.onclose(e => {
        alert(e);
    });

    $scope.doTurn = function (card) {
        if (cardCanBeSelected(card)) {
            if (card.type == 'action') {
                hubConnection.invoke("Turn", card.description);
            } else {
                hubConnection.invoke("Invent", card.id);
            }

            $scope.turnApllyed = true;
        }
    };

    $scope.sendDropInvent = function (card) {
        hubConnection.invoke("Invent", card.id);
    }


    $scope.startGame = function () {
        hubConnection.invoke("Begin");
    };

    $scope.endGame = function () {
        hubConnection.invoke("End");
    };
});
