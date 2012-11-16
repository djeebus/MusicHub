(function ($) {
    function viewModel() {
		this.songs = ko.observable([]);
		this.users = ko.observable([]);

		this.currentSongTitle = ko.observable();
		this.currentSongArtist = ko.observable();
		this.currentStatus = ko.observable();
		this.log = ko.observable([]);

		this.canPlay = ko.observable(false);
		this.canStop = ko.observable(false);

        this.play = function () {
            musicControl.play();
        }

        this.stop = function () {
            musicControl.stop();
        }

        this.love = function () {
            musicControl.love();
        }

        this.hate = function () {
            musicControl.hate();
        }

        this.next = function () {
            musicControl.next();
        }

        this.initMusicControl = function() {
            var musicControl = $.connection.musicControl;

            musicControl.log = function (text) {
                model.log.append(text);
            };

            musicControl.updateActiveUsers = function (data) {
                model.users(data.users);
            };

            musicControl.updateCurrentSong = function (data) {
                model.currentSongTitle(data.song.title);
                model.currentSongArtist(data.song.artist);
            };

            musicControl.updateStatus = function (data) {
                var isPlaying = data.status == 'playing';
                model.canPlay(!isPlaying);
                model.canStop(isPlaying);

                model.currentStatus(data.status);
            }

            $.connection.hub.start(function () {
                musicControl.requestStatus();
            });

            return musicControl;
        }

        var musicControl = this.initMusicControl();
	}

	var model = new viewModel();

	ko.applyBindings(model);

	function log(text) {
		$('.status').append($('<div />').text(text));
	}
})(jQuery);