package presenter

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo/options"

	"go/internal/pkg/storage"
)

// struct for storing data
type Sequence struct {
	Counter float64 `bson:"counter"`
}

var userCollection = storage.Db().Database("orderid_sequence").Collection("sequence")

func GetNextCounter(w http.ResponseWriter, r *http.Request) {

	w.Header().Set("Content-Type", "application/json")
	filter := bson.D{{"_id", "globalgo"}} // converting value to BSON type
	after := options.After                // for returning updated document
	upsert := true                        // create if doesn't exist
	returnOpt := options.FindOneAndUpdateOptions{
		Upsert:         &upsert,
		ReturnDocument: &after,
	}
	update := bson.D{{"$inc", bson.D{{"Counter", 1}}}}
	updateResult := userCollection.FindOneAndUpdate(context.TODO(), filter, update, &returnOpt)

	var seq Sequence
	_ = updateResult.Decode(&seq)

	json.NewEncoder(w).Encode(seq.Counter)
}

func SayHello(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "Hi !")
}
