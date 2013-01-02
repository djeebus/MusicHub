(function ($) {
    function libraryModel(library) {
        this.id = library.id;
        this.name = library.name;
        this.songCount = library.songCount;
    }

    function viewModel() {
		this.songs = ko.observable([]);
		this.users = ko.observable([]);
		this.libraries = ko.observable([]);

		this.currentSongTitle = ko.observable();
		this.currentSongArtist = ko.observable();
		this.currentStatus = ko.observable();
		this.log = ko.observable([]);

		this.canPlay = ko.observable(false);
		this.canStop = ko.observable(false);
		this.canSkip = ko.observable(false);

		this.play = function () {
		    musicControl.play();
		};

        this.stop = function () {
            musicControl.stop();
        };

        this.love = function () {
            musicControl.love();
        };

        this.hate = function () {
            musicControl.hate();
        };

        this.next = function () {
            musicControl.next();
        };

        this.initMusicControl = function () {
            var mcHub = $.connection.musicControl;

            mcHub.log = function (text) {
                model.log.append(text);
            };

            mcHub.updateActiveUsers = function (data) {
                model.users(data.users);
            };

            mcHub.updateCurrentSong = function (data) {
                model.currentSongTitle(data.song.title);
                model.currentSongArtist(data.song.artist);
            };

            mcHub.updateLibraries = function (data) {
                var models = [];
                for (var x = 0; x < data.length; x++) {
                    models.push(new libraryModel(data[x]));
                }
                model.libraries(models);
            };

            mcHub.updateStatus = function (data) {
                var isPlaying = data.status === 'Playing';
                model.canPlay(!isPlaying);
                model.canStop(isPlaying);
                model.canSkip(isPlaying);

                model.currentStatus(data.status);
            };

            $.connection.hub.start(function () {
                mcHub.requestStatus();
            });

            return mcHub;
        };

        var musicControl = this.initMusicControl();
	}

	var model = new viewModel();

	ko.applyBindings(model);

	function log(text) {
		$('.status').append($('<div />').text(text));
	}
})(jQuery);