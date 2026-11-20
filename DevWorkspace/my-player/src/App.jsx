import { useState, useRef, useEffect } from "react";

const VIDEO_POOL = [
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4", title: "Big Buck Bunny" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4", title: "Elephants Dream" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4", title: "For Bigger Blazes" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4", title: "For Bigger Escapes" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4", title: "For Bigger Fun" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerJoyrides.mp4", title: "For Bigger Joyrides" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerMeltdowns.mp4", title: "For Bigger Meltdowns" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/Sintel.mp4", title: "Sintel" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/SubaruOutbackOnStreetAndDirt.mp4", title: "Subaru Outback" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4", title: "Tears of Steel" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/VolkswagenGTIReview.mp4", title: "VW GTI Review" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/WeAreGoingOnBullrun.mp4", title: "We're On Bullrun" },
  { url: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/WhatCarCanYouGetForAGrand.mp4", title: "Car For A Grand" },
];

const COLS = { 2: 2, 4: 2, 6: 3, 8: 4 };

function pickRandom(pool, count) {
  return [...pool].sort(() => Math.random() - 0.5).slice(0, count);
}

function fmtTime(s) {
  if (!s || isNaN(s)) return "0:00";
  const m = Math.floor(s / 60);
  return `${m}:${Math.floor(s % 60).toString().padStart(2, "0")}`;
}

function VideoCell({ video, index, cellKey }) {
  const videoRef = useRef(null);
  const [playing, setPlaying] = useState(false);
  const [progress, setProgress] = useState(0);
  const [vol, setVol] = useState(0.7);
  const [muted, setMuted] = useState(true);
  const [dur, setDur] = useState(0);
  const [cur, setCur] = useState(0);
  const [hover, setHover] = useState(false);

  useEffect(() => {
    const v = videoRef.current;
    if (!v) return;
    v.volume = vol;
    v.muted = true;
  }, [cellKey]);

  const togglePlay = (e) => {
    e.stopPropagation();
    const v = videoRef.current;
    if (!v) return;
    playing ? v.pause() : v.play();
  };

  const handleSeek = (e) => {
    e.stopPropagation();
    const v = videoRef.current;
    if (!v || !dur) return;
    const rect = e.currentTarget.getBoundingClientRect();
    v.currentTime = ((e.clientX - rect.left) / rect.width) * dur;
  };

  const handleVol = (e) => {
    e.stopPropagation();
    const val = parseFloat(e.target.value);
    setVol(val);
    const v = videoRef.current;
    if (v) { v.volume = val; v.muted = val === 0; }
    setMuted(val === 0);
  };

  const toggleMute = (e) => {
    e.stopPropagation();
    const v = videoRef.current;
    if (!v) return;
    const nm = !muted;
    v.muted = nm;
    setMuted(nm);
  };

  const goFullscreen = (e) => {
    e && e.stopPropagation();
    videoRef.current?.requestFullscreen?.();
  };

  const volIcon = muted || vol === 0 ? "ti-volume-3" : vol < 0.5 ? "ti-volume" : "ti-volume-2";

  return (
    <div
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      onDoubleClick={goFullscreen}
      style={{
        background: "#080808",
        borderRadius: 6,
        overflow: "hidden",
        border: `1px solid ${hover ? "#333" : "#1a1a1a"}`,
        display: "flex",
        flexDirection: "column",
        transition: "border-color 0.15s",
      }}
    >
      <div style={{ position: "relative", background: "#000" }}>
        <div style={{
          position: "absolute", top: 6, left: 8, zIndex: 2,
          fontSize: 10, color: "#888", background: "rgba(0,0,0,0.75)",
          padding: "2px 7px", borderRadius: 3, pointerEvents: "none",
          fontFamily: "monospace", letterSpacing: "0.03em",
        }}>
          CH{String(index + 1).padStart(2, "0")} · {video.title}
        </div>

        {!playing && (
          <div onClick={togglePlay} style={{
            position: "absolute", inset: 0, display: "flex",
            alignItems: "center", justifyContent: "center",
            background: "rgba(0,0,0,0.3)", zIndex: 1, cursor: "pointer",
            opacity: hover ? 1 : 0, transition: "opacity 0.2s",
          }}>
            <div style={{
              width: 40, height: 40, borderRadius: "50%",
              background: "rgba(255,255,255,0.15)", display: "flex",
              alignItems: "center", justifyContent: "center",
              backdropFilter: "blur(4px)",
            }}>
              <i className="ti ti-player-play" style={{ fontSize: 20, color: "#fff" }} />
            </div>
          </div>
        )}

        <video
          key={cellKey}
          ref={videoRef}
          src={video.url}
          style={{ width: "100%", aspectRatio: "16/9", display: "block", cursor: "pointer" }}
          onPlay={() => setPlaying(true)}
          onPause={() => setPlaying(false)}
          onTimeUpdate={() => {
            const v = videoRef.current;
            if (!v) return;
            setCur(v.currentTime);
            setProgress(v.duration ? (v.currentTime / v.duration) * 100 : 0);
          }}
          onLoadedMetadata={() => {
            const v = videoRef.current;
            if (v) { setDur(v.duration); v.volume = vol; }
          }}
          muted={muted}
          preload="metadata"
          onClick={togglePlay}
        />
      </div>

      <div style={{
        background: "#0d0d0d",
        padding: "5px 8px 6px",
        borderTop: "1px solid #1a1a1a",
        display: "flex",
        flexDirection: "column",
        gap: 4,
      }}>
        <div
          onClick={handleSeek}
          style={{ height: 3, background: "#2a2a2a", borderRadius: 2, cursor: "pointer", position: "relative" }}
        >
          <div style={{
            width: `${progress}%`, height: "100%",
            background: "linear-gradient(90deg, #3b82f6, #60a5fa)",
            borderRadius: 2, transition: "width 0.1s linear",
          }} />
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: 4 }}>
          <button onClick={togglePlay} aria-label={playing ? "Pause" : "Play"} style={{
            background: "none", border: "none", color: "#bbb",
            cursor: "pointer", padding: "1px 3px", display: "flex", alignItems: "center",
          }}>
            <i className={`ti ${playing ? "ti-player-pause" : "ti-player-play"}`} style={{ fontSize: 14 }} />
          </button>

          <span style={{ fontSize: 10, color: "#555", fontFamily: "monospace", flex: 1 }}>
            {fmtTime(cur)} / {fmtTime(dur)}
          </span>

          <button onClick={toggleMute} aria-label="Toggle mute" style={{
            background: "none", border: "none", color: "#777",
            cursor: "pointer", padding: "1px 2px", display: "flex", alignItems: "center",
          }}>
            <i className={`ti ${volIcon}`} style={{ fontSize: 13 }} />
          </button>

          <input
            type="range" min="0" max="1" step="0.05"
            value={muted ? 0 : vol}
            onChange={handleVol}
            onClick={(e) => e.stopPropagation()}
            style={{ width: 44, cursor: "pointer", accentColor: "#3b82f6" }}
          />

          <button onClick={goFullscreen} aria-label="Fullscreen" style={{
            background: "none", border: "none", color: "#555",
            cursor: "pointer", padding: "1px 2px", display: "flex", alignItems: "center",
          }}>
            <i className="ti ti-maximize" style={{ fontSize: 12 }} />
          </button>
        </div>
      </div>
    </div>
  );
}

export default function MultiVideoPlayer() {
  const [count, setCount] = useState(4);
  const [videos, setVideos] = useState(() => pickRandom(VIDEO_POOL, 4));
  const [refreshKey, setRefreshKey] = useState(0);
  const [fading, setFading] = useState(false);

  const cols = COLS[count] || 2;

  const refresh = () => {
    setFading(true);
    setTimeout(() => {
      setVideos(pickRandom(VIDEO_POOL, count));
      setRefreshKey((k) => k + 1);
      setFading(false);
    }, 250);
  };

  const switchCount = (n) => {
    setCount(n);
    setFading(true);
    setTimeout(() => {
      setVideos(pickRandom(VIDEO_POOL, n));
      setRefreshKey((k) => k + 1);
      setFading(false);
    }, 250);
  };

  return (
    <div style={{ fontFamily: "var(--font-sans)", paddingBottom: 12 }}>
      <h2 className="sr-only">Multi-View Video Player</h2>

      <div style={{
        display: "flex", alignItems: "center", gap: 8,
        marginBottom: 10, padding: "8px 12px",
        background: "#0d0d0d", borderRadius: 8, border: "1px solid #1e1e1e",
      }}>
        <i className="ti ti-device-tv-old" style={{ fontSize: 16, color: "#3b82f6" }} aria-hidden="true" />
        <span style={{ fontSize: 13, fontWeight: 500, color: "#aaa", flex: 1, letterSpacing: "0.02em" }}>
          Multi-View Player
        </span>

        <span style={{ fontSize: 11, color: "#444", marginRight: 4 }}>channels:</span>
        <div style={{ display: "flex", gap: 3 }}>
          {[2, 4, 6, 8].map((n) => (
            <button key={n} onClick={() => switchCount(n)} style={{
              padding: "3px 9px", fontSize: 12, borderRadius: 4, cursor: "pointer",
              background: count === n ? "#1d4ed8" : "transparent",
              color: count === n ? "#e0eaff" : "#555",
              border: `1px solid ${count === n ? "#2563eb" : "#252525"}`,
              transition: "all 0.15s", fontWeight: count === n ? 500 : 400,
            }}>
              {n}
            </button>
          ))}
        </div>

        <button onClick={refresh} style={{
          display: "flex", alignItems: "center", gap: 5, marginLeft: 4,
          padding: "3px 10px", fontSize: 12, borderRadius: 4, cursor: "pointer",
          background: "transparent", color: "#777", border: "1px solid #252525",
          transition: "all 0.15s",
        }}>
          <i className="ti ti-refresh" style={{ fontSize: 13, animation: fading ? "spin 0.25s linear" : "none" }} aria-hidden="true" />
          Shuffle
        </button>
      </div>

      <div style={{
        display: "grid",
        gridTemplateColumns: `repeat(${cols}, 1fr)`,
        gap: 6,
        opacity: fading ? 0 : 1,
        transition: "opacity 0.2s",
      }}>
        {videos.map((video, i) => (
          <VideoCell
            key={`${refreshKey}-${i}`}
            cellKey={`${refreshKey}-${i}`}
            video={video}
            index={i}
          />
        ))}
      </div>

      <p style={{ fontSize: 11, color: "#333", textAlign: "center", marginTop: 10, marginBottom: 0 }}>
        double-click any video to fullscreen · videos muted by default
      </p>

      <style>{`
        @keyframes spin { to { transform: rotate(360deg); } }
        .sr-only { position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0,0,0,0); border: 0; }
      `}</style>
    </div>
  );
}