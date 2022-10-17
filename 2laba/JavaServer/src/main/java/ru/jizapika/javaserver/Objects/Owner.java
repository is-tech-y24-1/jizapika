package ru.jizapika.javaserver.Objects;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class Owner {
    private String name;
    private Date birthday;
    private List<Integer> kittens;

    public Owner(String name, Date birthday) {
        this.name = name;
        this.birthday = birthday;
        this.kittens = new ArrayList<>();
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public Date getBirthday() {
        return birthday;
    }

    public void setBirthday(Date birthday) {
        this.birthday = birthday;
    }

    public List<Integer> getKittens() {
        return kittens;
    }

    public void addKitten(int kittenId) {
        kittens.add(kittenId);
    }
}
