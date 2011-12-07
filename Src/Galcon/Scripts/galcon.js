// Canvas context
var ctx;
var galconHub;

var WIDTH;
var HEIGHT;
var canvasMinX;
var canvasMaxX;
var canvasMinY;
var canvasMaxY;

var clientId;

// galcon game object
var galconGame = {
    planets: [],
    ships: []
};

function drawCircle(x, y, radius, color) {
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, Math.PI * 2, true);
    ctx.closePath();
    ctx.fill();
}

function isInsideCanvas(evt) {
    if (evt.pageX > canvasMinX && evt.pageX < canvasMaxX && evt.pageY > canvasMinY && evt.pageY < canvasMaxY) {
        return true;
    } else {
        return false;
    }
}

function ring(x, y, r, color) {
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2, true);
    ctx.closePath();
    ctx.fill();
    ctx.fillStyle = "#000";
    ctx.lineWidth = 2;
    ctx.strokeStyle = "#000";
    ctx.stroke();
}

function Ship(x, y, dx, dy, targetPlanet) {
    this.x = x;
    this.y = y;
    this.dx = dx;
    this.dy = dy;

    this.draw = function () {

        this.x = this.x + this.dx;
        this.y = this.y + this.dy;

        ring(this.x, this.y, 10, "#FF0000");
    };

    this.isFinished = function () {

        if (isCollidingWithPlanet(this.x, this.y, targetPlanet)) {
            targetPlanet.numShips -= 5;
            return true;
        } else {
            return false;
        }

    };
}

function Planet(id, x, y, radius, numShips) {
    this.id = id;
    this.x = x;
    this.y = y;
    this.numShips = numShips;
    this.radius = radius;
    this.selected = false;
    this.owner = "";

    this.draw = function () {
        var color = "#333";
        if (this.owner == clientId) {
            color = "#0F0";
        } else if (this.owner != "") {
            color = "#F00";
        }

        drawCircle(x, y, radius, color);

        if (this.selected) {
            ctx.lineWidth = 4;
            ctx.strokeStyle = "#FFF";
            ctx.stroke();
        }

        ctx.fillStyle = "#000";
        ctx.font = "14pt Arial";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(this.numShips, this.x, this.y);
    };
}

function isCollidingWithPlanet(x, y, planet) {
    if (Math.pow((x - planet.x), 2) + Math.pow((y - planet.y), 2) < Math.pow(planet.radius, 2)) {
        return true;
    } else {
        return false;
    }
}

function onMouseClick(evt) {
    if (isInsideCanvas(evt)) {
        for (var i = 0; i < galconGame.planets.length; i++) {
            var mousePlanet = galconGame.planets[i];
            if (isCollidingWithPlanet(evt.pageX, evt.pageY, mousePlanet)) {
                if (mousePlanet.owner == clientId) {
                    mousePlanet.selected = !mousePlanet.selected;
                } else {
                    for (var j = 0; j < galconGame.planets.length; j++) {
                        if (galconGame.planets[j].selected == true) {
                            attack(galconGame.planets[j], mousePlanet);
                        }
                    }
                }
            }
        }
    }
}

function attack(sourcePlanet, targetPlanet) {
    galconHub.launchAttack(sourcePlanet.id, targetPlanet.id);
}

function resetGameCommand() {
    galconHub.resetGame();
}

$(function () {
    var canvas = document.getElementById('game');
    ctx = canvas.getContext('2d');
    WIDTH = $("#game").width();
    HEIGHT = $("#game").height();

    canvasMinX = $("#game").offset().left;
    canvasMaxX = canvasMinX + WIDTH;

    canvasMinY = $("#game").offset().top;
    canvasMaxY = canvasMinY + HEIGHT;

    $("#container").click(onMouseClick);

    $("#reset").click(resetGameCommand);

    // init
    // Proxy created on the fly
    galconHub = $.connection.galconHub;

    // Declare a function on the chat hub so the server can invoke it
    galconHub.initPlanets = function (planets) {
        console.log("initPlanets");
        $.each($.parseJSON(planets), function (idx, value) {
            var planet1 = new Planet(value.Id, value.X, value.Y, value.Radius, value.NumShips);
            planet1.owner = value.Owner.ClientId;
            galconGame.planets.push(planet1);
        });
    };

    galconHub.addShips = function (shipData) {
        console.log("addShips");
        var ship = $.parseJSON(shipData);
        galconGame.ships.push(new Ship(ship.X, ship.Y, ship.Dx, ship.Dy, galconGame.planets[ship.TargetPlanet.Id - 1]));
    };

    galconHub.refreshPlanets = function (planets) {
        console.log("refreshPlanets");
        $.each($.parseJSON(planets), function (idx, value) {
            galconGame.planets[idx].numShips = value.NumShips;
            if (galconGame.planets[idx].owner != value.Owner.ClientId && galconGame.planets[idx].selected) {
                galconGame.planets[idx].selected = false;
            }
            galconGame.planets[idx].owner = value.Owner.ClientId;
        });
    };

    galconHub.reJoin = function () {
        joinGame();
    };

    $.connection.hub.start(function () {
        joinGame();
    });

    $(window).unload(function () {
        galconHub.leave();
    });

    setInterval(gameloop, 33); // 33 milliseconds = ~ 30 frames per sec
});

function joinGame() {
    galconGame.planets = [];
    galconGame.ships = [];
    
    galconHub.join("HTML5")
                .fail(function (e) {
                    alert(e);
                })
                .done(function () {
                    clientId = $.connection.hub.clientId;
                    console.log("my clientId " + clientId);
                });
}

function gameloop() {
    // clear the canvas before re-drawing.
    clear(ctx);

    for (var j = 0; j < galconGame.planets.length; j++) {
        galconGame.planets[j].draw();
    }

    if (galconGame.ships.length > 0) {
        for (var i = 0; i < galconGame.ships.length; i++) {
            if (galconGame.ships[i].isFinished()) {
                galconGame.ships.splice(i, 1);
            } else {
                galconGame.ships[i].draw();
            }
        }
    }
}

function clear(ctx) {
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
}
