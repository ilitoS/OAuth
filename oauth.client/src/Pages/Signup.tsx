import axios, { AxiosError, AxiosResponse } from "axios";
import { useState } from "react";
import { Alert, Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router";

function Signup() {
    const [email, setEmail] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [invalidPasswordMatch, setInvalidPasswordMatch] = useState<boolean>(false);
    const [invalidEmail, setInvalidEmail] = useState<boolean>(false);
    const [invalidPassword, setInvalidPassword] = useState<boolean>(false);
    const [errorMessage, setErrorMessage] = useState<string>('');
    const [showError, setShowError] = useState<boolean>(false);
    const navigate = useNavigate();

    return (
        <Form className="login-form" noValidate >
            <Form.Label className="mb-3">Sign up</Form.Label>
            <div className="login-frame mb-3">
                {showError ? <Alert id="error-alert" variant='danger'>{errorMessage}</Alert> : <></>}
                <Form.Group className="mb-3 text-align-left" controlId="usernameContainer">
                    <Form.Label>Email:</Form.Label>
                    <Form.Control isInvalid={invalidEmail} required className="" type="email" placeholder="" onChange={handleEmail}></Form.Control>
                    <Form.Control.Feedback type="invalid">Email is required</Form.Control.Feedback>
                </Form.Group> 
                <Form.Group className="mb-3 text-align-left" controlId="passwordContainer">
                    <Form.Label>Password:</Form.Label>
                    <Form.Control isInvalid={invalidPassword} required type="password" placeholder="" onChange={handlePassword}></Form.Control>
                    <Form.Control.Feedback type="invalid">Password is required</Form.Control.Feedback>
                </Form.Group>
                <Form.Group className="mb-3 text-align-left" controlId="passwordConfirmContainer">
                    <Form.Label>Confirm Password:</Form.Label>
                    <Form.Control isInvalid={invalidPasswordMatch} required type="password" placeholder="" onChange={PasswordsMatch}></Form.Control>
                    <Form.Control.Feedback type="invalid">Passwords do not match</Form.Control.Feedback>
                </Form.Group>
                <Button className="w-100" variant="primary" type="button" onClick={handleSubmit}>Sign Up</Button>
            </div>
        </Form>
    );

    function PasswordsMatch(e: React.ChangeEvent<HTMLInputElement>) {
        if (password == e.target.value) {
            setInvalidPasswordMatch(false);
        } else {
            setInvalidPasswordMatch(true);
        }
    }

    function handleEmail(e: React.ChangeEvent<HTMLInputElement>) {
        const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        setEmail(e.target.value);

        if (emailRegex.test(email)) {
            setInvalidEmail(false);
        } else {
            setInvalidEmail(true);
        }
    }

    function handlePassword(e: React.ChangeEvent<HTMLInputElement>) {
        setPassword(e.target.value);

        if (password == '') {
            setInvalidPassword(true);
        } else {
            setInvalidPassword(false);
        }
    }
    
    function handleSubmit() {
        setErrorMessage('');
        setShowError(false);

        const data = { email: email, password: password };
        const headers = {
            "Content-Type": "application/json",
        };

        axios.post('https://localhost:5173/register', JSON.stringify(data), { headers })
            .then((response: AxiosResponse) => {
                setErrorMessage('');
                setShowError(false);
                
                navigate(response.data.redirectUrl);
            })
            .catch((error: AxiosError) => {
                setShowError(true);
                setErrorMessage(error.response?.data);
            });
    }
}

export default Signup;