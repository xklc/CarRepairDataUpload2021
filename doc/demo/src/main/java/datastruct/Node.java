package datastruct;

public class Node {
    public int data;
    public Node next;

    public boolean isHuiWen(Node head){
        Node cur = head;
        Node newHead = null;
        Node tmp = head;
        tmp.next = null;
        while (cur.next!=null){
            tmp = cur;
            cur = cur.next;


        }
        Node tail = cur;

        return false;
    }
    public static void main(String[] args) {

    }
}
