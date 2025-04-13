from fastapi import FastAPI
from fastapi.responses import StreamingResponse
import httpx

app = FastAPI()

HLS_MAP = {
    "bbc_6music": "http://as-hls-ww-live.akamaized.net/pool_81827798/live/ww/bbc_6music/bbc_6music.isml/bbc_6music-audio=320000.norewind.m3u8",
    "bbc_asian_network": "http://as-hls-ww-live.akamaized.net/pool_22108647/live/ww/bbc_asian_network/bbc_asian_network.isml/bbc_asian_network-audio=128000.norewind.m3u8",
}

@app.get("/hls/{station}.m3u8")
async def get_playlist(station: str):
    url = HLS_MAP.get(station)
    if not url:
        return {"error": "Station not found"}

    async with httpx.AsyncClient() as client:
        r = await client.get(url)
        playlist = r.text

        # Rewrite chunk references in the playlist
        base_path = url.rsplit("/", 1)[0]
        rewritten = playlist.replace("\n", "\n").splitlines()
        modified = []

        for line in rewritten:
            if line.strip().endswith(".ts"):
                filename = line.strip()
                # Repoint chunk through proxy
                proxied = f"/hls/chunk?station={station}&filename={filename}"
                modified.append(proxied)
            else:
                modified.append(line)

        return StreamingResponse(
            iter(["\n".join(modified)]),
            media_type="application/vnd.apple.mpegurl"
        )

@app.get("/hls/chunk")
async def get_chunk(station: str, filename: str):
    base_url = HLS_MAP.get(station)
    if not base_url:
        return {"error": "Station not found"}

    chunk_url = base_url.rsplit("/", 1)[0] + "/" + filename

    async with httpx.AsyncClient() as client:
        r = await client.get(chunk_url)
        return StreamingResponse(r.aiter_bytes(), media_type="video/MP2T")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=5006)