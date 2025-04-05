import { Button, Spinner } from "react-bootstrap";
import axios, { AxiosError, AxiosResponse } from "axios";
import { Navigate, useNavigate } from "react-router";
import { useEffect, useState } from 'react';

function Home() {
    const navigate = useNavigate();
    const [isAuthorized, setIsAuthorized] = useState<boolean>(false);
    const [isLoading, setLoading] = useState<boolean>(true);
    const [email, setEmail] = useState<string>('');
 
    useEffect(() => {
        async function loginCheck() {
            const response = await fetch("https://localhost:5173/login-check");

            if (response.status == 200) {
                setIsAuthorized(true);
                const data = await response.json();
                setEmail(data.email);
            }
        };

        loginCheck()
            .catch((error) => {
                console.log(error);
            })
            .finally(() => {
                setLoading(false);
            })
    }, []);

    return (
        <div>
        {isLoading ? 
            <Spinner animation="border" role="status"></Spinner> 
                : 
                isAuthorized
                    ? 
                    <div>
                        <p>{email} is logged in!</p>
                        <Button className="mb-3 w-100" onClick={logout}>Log out</Button>
                    </div>
                    :
                    <Navigate to="/login"></Navigate>
        }
        </div>
            
        
    );

    function logout() {
        axios.post('https://localhost:5173/logout')
            .then((response: AxiosResponse) => {
                navigate(response.data.redirectUrl);
            })
            .catch((error: AxiosError) => {
            });
    }
}

export default Home;