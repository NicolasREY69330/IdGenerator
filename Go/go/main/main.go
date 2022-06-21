package main

import (
	"log"
	"net/http"

	"github.com/gorilla/mux"

	presenter "go/internal/presenter"
)

func main() {

	route := mux.NewRouter()
	s := route.PathPrefix("/api").Subrouter() //Base Path

	//Routes
	s.HandleFunc("/getNextCounter", presenter.GetNextCounter).Methods("GET")
	s.HandleFunc("/hello", presenter.SayHello).Methods("GET")
	log.Fatal(http.ListenAndServe(":8000", s)) // Run Server
}
