package test;

public class StringTest {
    public static  void test1(String str){
        for (int idx=str.length()-1; idx>=0; idx=idx-1){
            System.out.print(str.charAt(idx));
        }
    }

    public static void test2(String str){
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.append(str);
        System.out.println(stringBuilder.reverse());
    }
    public static void main(String[] args) {
        String str = "abcdefg";
        test1(str);
        System.out.println();
        test2(str);
    }
}
