package ru.jizapika.javaserver.Objects;

import java.util.*;

public class Kitten {
    private String name;
    private Date birthday;
    private String breed;
    private String color;
    private int ownerId;
    private List<Integer> friends;

    public Kitten(String name, Date birthday, String breed, String color, int ownerId) {
        this.name = name;
        this.birthday = birthday;
        this.breed = breed;
        this.color = color;
        this.ownerId = ownerId;
        this.friends = new ArrayList<>();
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

    public String getBreed() {
        return breed;
    }

    public void setBreed(String breed) {
        this.breed = breed;
    }

    public String getColor() {
        return color;
    }

    public void setColor(String color) {
        this.color = color;
    }

    public int getOwnerId() {
        return ownerId;
    }

    public void setOwnerId(int ownerId) {
        this.ownerId = ownerId;
    }

    public List<Integer> getFriends() {
        return friends;
    }

    public void addFriend(int friendId) {
        friends.add(friendId);
    }
}
