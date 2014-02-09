package ManicDigger.lib;

public class GamePlatformNative extends GamePlatform {
    @Override
    public String charArrayToString(int[] charArray, int length) {
        return null;
    }

    @Override
    public void clipboardSetText(String s) {

    }

    @Override
    public TextTexture createTextTexture(Gl gl, String text, float fontSize) {
        return null;
    }

    @Override
    public String[] directoryGetFiles(String path, IntRef length) {
        return new String[0];
    }

    @Override
    public void exit() {

    }

    @Override
    public String fileName(String fullpath) {
        return null;
    }

    @Override
    public float floatParse(String value) {
        return 0;
    }

    @Override
    public int floatToInt(float value) {
        return 0;
    }

    @Override
    public String getFullFilePath(String filename) {
        return null;
    }

    @Override
    public int intParse(String value) {
        return 0;
    }

    @Override
    public String intToString(int value) {
        return null;
    }

    @Override
    public float mathSqrt(float value) {
        return 0;
    }

    @Override
    public String pathSavegames() {
        return null;
    }

    @Override
    public String stringFormat(String format, String arg0) {
        return null;
    }

    @Override
    public String stringFormat2(String format, String arg0, String arg1) {
        return null;
    }

    @Override
    public String[] stringSplit(String value, String separator, IntRef returnLength) {
        return new String[0];
    }

    @Override
    public int[] stringToCharArray(String s, IntRef length) {
        return new int[0];
    }

    @Override
    public String stringTrim(String value) {
        return null;
    }

    @Override
    public void textSize(String text, float fontSize, IntRef outWidth, IntRef outHeight) {

    }

    @Override
    public String timestamp() {
        return null;
    }

    @Override
    public void webClientDownloadStringAsync(String url, HttpResponseCi response) {

    }
}
