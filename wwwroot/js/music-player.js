class MusicPlayer {
    constructor() {
        // Elements
        this.audio = document.getElementById('audioPlayer');
        this.playPauseBtn = document.getElementById('playPauseBtn');
        this.prevBtn = document.getElementById('prevBtn');
        this.nextBtn = document.getElementById('nextBtn');
        this.shuffleBtn = document.getElementById('shuffleBtn');
        this.repeatBtn = document.getElementById('repeatBtn');
        this.volumeBtn = document.getElementById('volumeBtn');
        this.volumeSlider = document.getElementById('volumeSlider');
        this.progressBarContainer = document.getElementById('progressBarContainer');
        this.progressBarFill = document.getElementById('progressBarFill');
        this.currentTimeEl = document.getElementById('currentTime');
        this.durationEl = document.getElementById('duration');
        this.playerSongImage = document.getElementById('playerSongImage');
        this.playerSongTitle = document.getElementById('playerSongTitle');
        this.playerSongArtist = document.getElementById('playerSongArtist');

        // State
        this.playlist = [];
        this.currentIndex = 0;
        this.isPlaying = false;
        this.isShuffle = false;
        this.repeatMode = 0;
        this.isUserVip = !!document.body.dataset.isVip;

        this.init();
    }

    init() {
        this.loadPlaylist();
        this.loadState();
        this.attachEvents();
        this.loadSongsFromPage();
        const savedVolume = localStorage.getItem('playerVolume') || 70;
        this.audio.volume = savedVolume / 100;
        this.volumeSlider.value = savedVolume;
        this.restorePlayback();
    }

    canPlay(song) {
        if (song.isVip && !this.isUserVip) {
            alert("Bài hát này chỉ dành cho người dùng VIP!");
            return false;
        }
        return true;
    }

    loadPlaylist() {
        const data = localStorage.getItem('musicPlayerPlaylist');
        if (data) this.playlist = JSON.parse(data);
    }

    savePlaylist() {
        localStorage.setItem('musicPlayerPlaylist', JSON.stringify(this.playlist));
    }

    loadState() {
        const data = localStorage.getItem('musicPlayerState');
        if (data) {
            const state = JSON.parse(data);
            this.currentIndex = state.currentIndex || 0;
            this.isShuffle = state.isShuffle || false;
            this.repeatMode = state.repeatMode || 0;
            if (this.isShuffle) this.shuffleBtn.classList.add('active');
            if (this.repeatMode) this.repeatBtn.classList.add('active');
        }
    }

    saveState() {
        localStorage.setItem('musicPlayerState', JSON.stringify({
            currentIndex: this.currentIndex,
            isShuffle: this.isShuffle,
            repeatMode: this.repeatMode,
            currentTime: this.audio.currentTime,
            isPlaying: this.isPlaying,
            currentSrc: this.audio.src
        }));
    }

    restorePlayback() {
        const data = localStorage.getItem('musicPlayerState');
        if (!data || this.playlist.length === 0) return;
        const state = JSON.parse(data);
        const song = this.playlist[state.currentIndex];
        if (!this.canPlay(song)) return;  // Chặn nhạc VIP
        this.currentIndex = state.currentIndex;
        this.loadSong(this.currentIndex);
        this.audio.addEventListener('loadedmetadata', () => {
            this.audio.currentTime = state.currentTime || 0;
            if (state.isPlaying) this.play();
        }, { once: true });
    }

    loadSongsFromPage() {
        const cards = document.querySelectorAll('.song-card, [data-song-id]');
        const ids = new Set(this.playlist.map(s => s.id));
        cards.forEach((card, i) => {
            const song = {
                id: card.dataset.songId || `page-${i}`,
                title: card.dataset.title || card.querySelector('.song-title')?.textContent || 'Unknown',
                artist: card.dataset.artist || card.querySelector('.song-artist')?.textContent || 'Unknown Artist',
                audio: card.dataset.audio || card.dataset.audioUrl,
                image: card.dataset.image || card.querySelector('img')?.src || '/images/default-song.png',
                isVip: card.dataset.isVip === 'true'
            };
            if (song.audio && !ids.has(song.id)) {
                this.playlist.push(song); 
                ids.add(song.id);
            }
            card.style.cursor = 'pointer';
            card.addEventListener('click', e => {
                if (e.target.tagName === 'A' || e.target.tagName === 'BUTTON') return;
                const idx = this.playlist.findIndex(s => s.id === song.id);
                if(idx !== -1) this.playSongByIndex(idx);
            });
        });
        this.savePlaylist();
    }

    attachEvents() {
        this.playPauseBtn.addEventListener('click', ()=>this.togglePlayPause());
        this.nextBtn.addEventListener('click', ()=>this.playNext());
        this.prevBtn.addEventListener('click', ()=>this.playPrevious());
        this.shuffleBtn.addEventListener('click', ()=>this.toggleShuffle());
        this.repeatBtn.addEventListener('click', ()=>this.toggleRepeat());
        this.volumeSlider.addEventListener('input', e=>this.changeVolume(e.target.value));
        this.volumeBtn.addEventListener('click', ()=>this.toggleMute());
        this.progressBarContainer.addEventListener('click', e=>this.seekTo(e));
        this.audio.addEventListener('timeupdate', ()=>this.updateProgress());
        this.audio.addEventListener('loadedmetadata', ()=>this.updateDuration());
        this.audio.addEventListener('ended', ()=>this.onSongEnd());
        this.audio.addEventListener('play', ()=>{this.isPlaying=true; this.updatePlayPause(); this.saveState();});
        this.audio.addEventListener('pause', ()=>{this.isPlaying=false; this.updatePlayPause(); this.saveState();});
        window.addEventListener('beforeunload', ()=>{this.saveState(); this.savePlaylist();});
    }

    loadSong(index){
        if(!this.playlist[index]) return;
        const s = this.playlist[index];
        this.audio.src = s.audio;
        this.playerSongTitle.textContent = s.title;
        this.playerSongArtist.textContent = s.artist;
        this.playerSongImage.src = s.image;
        this.saveState();
    }

    playSongByIndex(i){
        const song = this.playlist[i];
        if(!this.canPlay(song)) return;
        this.currentIndex = i;
        this.loadSong(i);
        this.play();
    }

    play() {
        const currentSong = this.playlist[this.currentIndex];
        if(!this.canPlay(currentSong)) return;
        this.audio.play().then(() => {
            if (currentSong && currentSong.id && !isNaN(currentSong.id)) {
                fetch(`/Song/IncreasePlayCount?id=${currentSong.id}`, { method: 'POST' });
            }
        }).catch(()=>{});
    }

    pause() { this.audio.pause(); }
    togglePlayPause() { this.isPlaying ? this.pause() : this.play(); }

    playNext() {
        let nextIndex = this.isShuffle 
            ? Math.floor(Math.random() * this.playlist.length) 
            : (this.currentIndex + 1) % this.playlist.length;

        const nextSong = this.playlist[nextIndex];
        if(!this.canPlay(nextSong)) return;

        this.currentIndex = nextIndex;
        this.loadSong(this.currentIndex);
        this.play();
    }

    playPrevious() {
        let prevIndex = this.audio.currentTime > 3 
            ? this.currentIndex 
            : (this.currentIndex - 1 + this.playlist.length) % this.playlist.length;

        const prevSong = this.playlist[prevIndex];
        if(!this.canPlay(prevSong)) return;

        this.currentIndex = prevIndex;
        this.loadSong(this.currentIndex);
        this.play();
    }

    toggleShuffle() { this.isShuffle = !this.isShuffle; this.shuffleBtn.classList.toggle('active', this.isShuffle); this.saveState(); }
    toggleRepeat() { this.repeatMode = (this.repeatMode + 1) % 3; this.repeatBtn.classList.toggle('active', this.repeatMode > 0); this.saveState(); }

    onSongEnd() {
        if(this.repeatMode === 2) { 
            this.audio.currentTime = 0; 
            this.play(); 
        } else if(this.repeatMode === 1) {
            this.playNext(); 
        } else if(this.currentIndex < this.playlist.length-1 || this.isShuffle) {
            this.playNext(); 
        } else {
            this.pause();
        }
    }

    changeVolume(v) { this.audio.volume = v/100; localStorage.setItem('playerVolume', v); this.updateVolumeIcon(); }
    toggleMute() {
        const vol = this.audio.volume;
        if(vol > 0) {
            this.audio.dataset.prevVolume = vol;
            this.audio.volume = 0;
            this.volumeSlider.value = 0;
        } else {
            const prev = this.audio.dataset.prevVolume || 0.7;
            this.audio.volume = prev;
            this.volumeSlider.value = prev*100;
        }
        this.updateVolumeIcon();
    }
    updateVolumeIcon() {
        const v = this.audio.volume, icon = this.volumeBtn.querySelector('i');
        icon.className = v===0?'bi bi-volume-mute-fill fs-5':v<0.5?'bi bi-volume-down-fill fs-5':'bi bi-volume-up-fill fs-5';
    }

    updateProgress() {
        if(this.audio.duration){
            const p=this.audio.currentTime/this.audio.duration*100;
            this.progressBarFill.style.width=p+'%';
            this.currentTimeEl.textContent=this.formatTime(this.audio.currentTime);
        }
    }
    updateDuration(){if(this.audio.duration) this.durationEl.textContent=this.formatTime(this.audio.duration);}
    seekTo(e){if(!this.audio.duration) return; const r=this.progressBarContainer.getBoundingClientRect(),p=(e.clientX-r.left)/r.width; this.audio.currentTime=p*this.audio.duration;}
    updatePlayPause(){this.playPauseBtn.querySelector('i').className=this.isPlaying?'bi bi-pause-fill fs-4':'bi bi-play-fill fs-4';}
    formatTime(s){const m=Math.floor(s/60),sec=Math.floor(s%60); return `${m}:${sec.toString().padStart(2,'0')}`;}
}

document.addEventListener('DOMContentLoaded', ()=>{ window.musicPlayer = new MusicPlayer(); });

// API helper
window.addToPlayer = function(songData){
    if(window.musicPlayer && !window.musicPlayer.playlist.find(s=>s.id===songData.id)){
        window.musicPlayer.playlist.push(songData); 
        window.musicPlayer.savePlaylist();
    }
}
window.playNow = function(songData){
    if(window.musicPlayer){
        const i=window.musicPlayer.playlist.findIndex(s=>s.id===songData.id);
        i!==-1 ? window.musicPlayer.playSongByIndex(i) :
        (window.musicPlayer.playlist.push(songData),
        window.musicPlayer.savePlaylist(),
        window.musicPlayer.playSongByIndex(window.musicPlayer.playlist.length-1));
    }
}
