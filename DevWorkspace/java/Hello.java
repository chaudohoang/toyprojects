import javax.swing.*;
import java.awt.*;

public class Hello extends JFrame {

    private final JLabel  lblMessage = new JLabel("");
    private final JButton btnHello   = new JButton("Say Hello");

    public Hello() {
        setTitle("Hello WinForms - Java");
        setSize(320, 180);
        setDefaultCloseOperation(EXIT_ON_CLOSE);
        setLayout(null);

        lblMessage.setBounds(20, 20, 260, 24);
        btnHello  .setBounds(20, 60, 120, 30);

        btnHello.addActionListener(e ->
            lblMessage.setText("Hello, World!"));

        add(lblMessage);
        add(btnHello);
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() ->
            new Hello().setVisible(true));
    }
}