from flask import Flask, request, jsonify
import requests

app = Flask(__name__)

station_urls = {
    "absolute70s": "https://edge-bauerall-01-gos2.sharp-stream.com/absolute70shigh.aac",
    "absolute80s": "https://edge-bauerall-01-gos2.sharp-stream.com/absolute80shigh.aac",
    "absolute90s": "https://edge-bauerall-01-gos2.sharp-stream.com/absolute90shigh.aac",
    "absoluteclassicrock": "http://edge-bauerall-01-gos2.sharp-stream.com/absoluteclassicrockhigh.aac",
    "absoluteradio": "http://www.radiofeeds.net/playlists/bauerflash.pls?station=absoluteradio-mp3",
    "bbcasiannetwork": "http://as-hls-ww-live.akamaized.net/pool_22108647/live/ww/bbc_asian_network/bbc_asian_network.isml/bbc_asian_network-audio%3d128000.norewind.m3u8",
    "bbcradio1": "http://a.files.bbci.co.uk/ms6/live/3441A116-B12E-4D2F-ACA8-C1984642FA4B/audio/simulcast/hls/nonuk/pc_hd_abr_v2/ak/bbc_radio_one.m3u8",
    "bbcradio1xtra": "http://as-hls-ww-live.akamaized.net/pool_92079267/live/ww/bbc_1xtra/bbc_1xtra.isml/bbc_1xtra-audio%3d128000.norewind.m3u8",
    "bbcradio2": "https://as-hls-ww.live.cf.md.bbci.co.uk/pool_74208725/live/ww/bbc_radio_two/bbc_radio_two.isml/bbc_radio_two-audio=96000-272331559.ts",
    "bbcradio3": "http://lstn.lv/bbcradio.m3u8?station=bbc_radio_three%22&bitrate=320000%22",
    "bbcradio4": "http://lstn.lv/bbcradio.m3u8?station=bbc_radio_fourfm%22&bitrate=320000%22",
    "bbcradio4extra": "http://lstn.lv/bbcradio.m3u8?station=bbc_radio_four_extra%&bitrate=320000",
    "bbcradio5live": "http://lstn.lv/bbcradio.m3u8?station=bbc_radio_five_live&bitrate=96000",
    "bbcradio5livesportsextra": "http://lstn.lv/bbcradio.m3u8?station=bbc_radio_five_live_sports_extra&bitrate=320000",
    "bbcradio6music": "http://as-hls-ww-live.akamaized.net/pool_81827798/live/ww/bbc_6music/bbc_6music.isml/bbc_6music-audio%3d320000.norewind.m3u8",
    "bbcradioscotland": "http://as-hls-ww-live.akamaized.net/pool_43322914/live/ww/bbc_radio_scotland_fm/bbc_radio_scotland_fm.isml/bbc_radio_scotland_fm-audio%3d128000.norewind.m3u8",
    "bbcradioworldservice": "https://stream.live.vc.bbcmedia.co.uk/bbc_world_service;",
    "capitaluk": "http://icecast.thisisdax.com/CapitalUKMP3",
    "capitalxtra": "http://media-the.musicradio.com/CapitalXTRALondonMP3",
    "classicfm": "http://icecast.thisisdax.com/ClassicFMMP3",
    "goldfm": "http://media-the.musicradio.com/GoldMP3",
    "greatesthits": "http://edge-bauerall-01-gos2.sharp-stream.com/net2london.mp3",
    "heart": "http://ice-the.musicradio.com:80/HeartUKMP3",
    "heart80s": "http://media-the.musicradio.com:80/Heart80sMP3",
    "heat": "https://edge-bauerall-01-gos2.sharp-stream.com/heat.mp3",
    "hitsradio": "http://edge-bauerall-01-gos2.sharp-stream.com/hits.aac",
    "jazzfm": "https://edge-bauerall-01-gos2.sharp-stream.com/jazzhigh.aac",
    "kerrang": "https://edge-bauerall-01-gos2.sharp-stream.com/kerrang.mp3",
    "kisstory": "https://edge-bauerall-01-gos2.sharp-stream.com/kisstory.aac",
    "kissfresh": "http://live-bauer-al.sharp-stream.com/kissfresh.mp3",
    "lbc": "https://media-ssl.musicradio.com/LBCUK",
    "magicchilled": "https://edge-bauerall-01-gos2.sharp-stream.com/magicchilled.aac",
    "magicfm": "http://edge-bauermz-01-gos2.sharp-stream.com/magic1054.mp3?aw_0_1st.skey=1657298076",
    "magicsoul": "https://edge-bauerall-01-gos2.sharp-stream.com/magicsoul.aac",
    "mellowmagic": "https://edge-bauerall-01-gos2.sharp-stream.com/magicmellow.aac",
    "planetoldies": "https://planetoldies-libstrm.radioca.st/stream",
    "planetrock": "https://edge-bauerall-01-gos2.sharp-stream.com/planetrock.aac",
    "radiox": "https://media-ice.musicradio.com/RadioXLondonMP3",
    "smooth": "http://icecast.thisisdax.com/SmoothUKMP3",
    "smoothextra": "http://icecast.thisisdax.com/SmoothLondon.m3u",
    "sunrise": "https://direct.sharp-stream.com/sunriseradio.mp3",
    "talkradio": "http://radio.talkradio.co.uk/stream-mobile?ref=rf",
    "talksport": "http://radio.talksport.com/stream?aisGetOriginalStream=true",
    "talksport2": "http://radio.talksport.com/stream2?awparams=platform:ts-web&amsparams=playerid:ts-web",
    "timesradio": "https://timesradio.wireless.radio/stream",
    "virgin": "https://radio.virginradio.co.uk/stream?aw_0_1st.platform=vr-web",
}

@app.route("/metadata")
def get_metadata():
    station = request.args.get("station")
    if not station or station not in station_urls:
        return jsonify({"error": "Invalid or missing station parameter"}), 400

    stream_url = station_urls[station]
    headers = {"Icy-MetaData": "1", "User-Agent": "Mozilla/5.0"}

    try:
        response = requests.get(stream_url, headers=headers, stream=True, timeout=5)
       icy_metaint_header = response.headers.get("icy-metaint")

        if icy_metaint_header is None:
            return jsonify({"error": "No icy-metaint header in response"}), 500

        metaint = int(icy_metaint_header)
        stream = response.raw
        stream.read(metaint)  # Skip audio data
        metadata_length = int.from_bytes(stream.read(1), "big") * 16
        metadata = stream.read(metadata_length).decode(errors='ignore')

        now_playing = ""
        if "StreamTitle='" in metadata:
            start = metadata.find("StreamTitle='") + len("StreamTitle='")
            end = metadata.find("';", start)
            now_playing = metadata[start:end]

        return jsonify({
            "station": station,
            "now_playing": now_playing,
            "icy-metaint": icy_metaint_header,
            "icy-name": response.headers.get("icy-name"),
            "icy-description": response.headers.get("icy-description"),
            "icy-genre": response.headers.get("icy-genre"),
            "icy-br": response.headers.get("icy-br"),
        })

    except Exception as e:
        return jsonify({"error": str(e)}), 500
  try:
        response = requests.get(stream_url, headers=headers, stream=True, timeout=5)
        icy_metaint_header = response.headers.get("icy-metaint")

        if icy_metaint_header is None:
            return jsonify({"error": "No icy-metaint header in response"}), 500

        metaint = int(icy_metaint_header)
        stream = response.raw
        stream.read(metaint)  # Skip audio data
        metadata_length = int.from_bytes(stream.read(1), "big") * 16
        metadata = stream.read(metadata_length).decode(errors='ignore')

        now_playing = ""
        if "StreamTitle='" in metadata:
            start = metadata.find("StreamTitle='") + len("StreamTitle='")
            end = metadata.find("';", start)
            now_playing = metadata[start:end]

        return jsonify({
            "station": station,
            "now_playing": now_playing,
            "icy-metaint": icy_metaint_header,
            "icy-name": response.headers.get("icy-name"),
            "icy-description": response.headers.get("icy-description"),
            "icy-genre": response.headers.get("icy-genre"),
            "icy-br": response.headers.get("icy-br"),
        })

    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5005)