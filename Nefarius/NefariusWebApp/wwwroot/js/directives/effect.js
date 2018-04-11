app.directive('effect',function () {
	return {
		restrict: "E",
		scope: {
            effectsList: '=',
            dir: '=',
            owner: '='
        },
        templateUrl: 'templates/effect.html',
		link: function (scope, element, attrs, ngModel) {
            var icons = {
                coin: 'fa-dollar-sign',
                spy: 'fa-male',
                invented: 'fa-info',
                inventions: 'fa-info',
                invention: 'fa-info'
            }
            
            scope.getEffectIcon = function(item){
                return icons[item];
            }
            
            scope.secondPart = function(effect){
                return effect.count.length != 1;
            }
		}
	};
});